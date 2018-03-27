using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
