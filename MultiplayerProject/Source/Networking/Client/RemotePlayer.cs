using Microsoft.Xna.Framework;
using NetworkPrediction;
using System;

namespace MultiplayerProject.Source
{
    public class RemotePlayer : Player
    {
        // This is the latest master copy of the tank state, used by our local
        // physics computations and prediction. This state will jerk whenever
        // a new network packet is received.
        ObjectState simulationState;

        // This is a copy of the state from immediately before the last
        // network packet was received.
        ObjectState previousState;

        private float _currentSmoothing;
        // Averaged time difference from the last 100 incoming packets, used to
        // estimate how our local clock compares to the time on the remote machine.
        RollingAverage clockDelta = new RollingAverage(100);

        private PlayerUpdatePacket _updatePacket;

        public void SetUpdatePacket(PlayerUpdatePacket updatePacket)
        {
            _updatePacket = updatePacket;
            simulationState.Rotation = _updatePacket.Rotation;
            simulationState.Position = new Vector2(_updatePacket.XPosition, _updatePacket.YPosition);
            simulationState.Speed = _updatePacket.Speed;

            if (Application.APPLY_ENTITY_INTERPOLATION)
            {
                // Start a new smoothing interpolation from our current
                // state toward this new state we just received.
                previousState = PlayerState;
                _currentSmoothing = 1;
            }
            else
            {
                _currentSmoothing = 0;
            }

            // TODO: APPLY PREDICTION
            //ApplyPrediction(gameTime, latency, packetSendTime);
        }

        public void UpdateRemote(int framesBetweenPackets, float deltaTime)
        {
            // Update the smoothing amount, which interpolates from the previous
            // state toward the current simultation state. The speed of this decay
            // depends on the number of frames between packets: we want to finish
            // our smoothing interpolation at the same time the next packet is due.
            float smoothingDecay = 1.0f / framesBetweenPackets;

            _currentSmoothing -= smoothingDecay;

            if (_currentSmoothing < 0)
                _currentSmoothing = 0;

            if (Application.APPLY_ENTITY_INTERPOLATION && _updatePacket != null)
            {
                // Predict how the remote tank will move by updating
                // our local copy of its simultation state.
                ApplyInputToPlayer(ref simulationState, _updatePacket.Input, deltaTime);
                Update(ref simulationState, deltaTime);

                // If both smoothing and prediction are active,
                // also apply prediction to the previous state.
                if (_currentSmoothing > 0)
                {
                    ApplyInputToPlayer(ref previousState, _updatePacket.Input, deltaTime);
                    Update(ref previousState, deltaTime);
                }
            }

            if (_currentSmoothing > 0)
            {
                // Interpolate the display state gradually from the
                // previous state to the current simultation state.
                ApplySmoothing();
            }
            else
            {
                // Copy the simulation state directly into the display state.
                PlayerState = simulationState;
            }
        }

        private void ApplySmoothing()
        {
            PlayerState.Position = Vector2.Lerp(simulationState.Position,
                                                 previousState.Position,
                                                 _currentSmoothing);

            PlayerState.Rotation = MathHelper.Lerp(simulationState.Rotation,
                                                        previousState.Rotation,
                                                        _currentSmoothing);

            PlayerState.Speed = MathHelper.Lerp(simulationState.Speed,
                                                          previousState.Speed,
                                                          _currentSmoothing);
        }

        /// <summary>
        /// Incoming network packets tell us where the tank was at the time the packet
        /// was sent. But packets do not arrive instantly! We want to know where the
        /// tank is now, not just where it used to be. This method attempts to guess
        /// the current state by figuring out how long the packet took to arrive, then
        /// running the appropriate number of local updates to catch up to that time.
        /// This allows us to figure out things like "it used to be over there, and it
        /// was moving that way while turning to the left, so assuming it carried on
        /// using those same inputs, it should now be over here".
        /// </summary>
        private void ApplyPrediction(GameTime gameTime, TimeSpan latency, float packetSendTime)
        {
            // Work out the difference between our current local time
            // and the remote time at which this packet was sent.
            float localTime = (float)gameTime.TotalGameTime.TotalSeconds;

            float timeDelta = localTime - packetSendTime;

            // Maintain a rolling average of time deltas from the last 100 packets.
            clockDelta.AddValue(timeDelta);

            // The caller passed in an estimate of the average network latency, which
            // is provided by the XNA Framework networking layer. But not all packets
            // will take exactly that average amount of time to arrive! To handle
            // varying latencies per packet, we include the send time as part of our
            // packet data. By comparing this with a rolling average of the last 100
            // send times, we can detect packets that are later or earlier than usual,
            // even without having synchronized clocks between the two machines. We
            // then adjust our average latency estimate by this per-packet deviation.

            float timeDeviation = timeDelta - clockDelta.AverageValue;

            latency += TimeSpan.FromSeconds(timeDeviation);

            TimeSpan oneFrame = TimeSpan.FromSeconds(1.0 / 60.0);

            // Apply prediction by updating our simulation state however
            // many times is necessary to catch up to the current time.
            while (latency >= oneFrame)
            {
                ApplyInputToPlayer(ref simulationState, _updatePacket.Input, (float)latency.TotalSeconds);
                Update(ref simulationState, (float)latency.TotalSeconds);

                latency -= oneFrame;
            }
        }
    }
}
