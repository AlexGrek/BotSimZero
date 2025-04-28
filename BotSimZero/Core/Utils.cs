using SimuliEngine.World;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.Core
{
    public static class Utils
    {
        /// <summary>
        /// Smoothly interpolates a Vector3 value from the current position to the target position over time.
        /// This method uses a critically damped spring-damper model to achieve smooth motion.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
        /// <param name="current">The current position of the object.</param>
        /// <param name="target">The target position to move towards.</param>
        /// <param name="currentVelocity">A reference to the current velocity of the object, which is updated by this method.</param>
        /// <param name="smoothTime">The time it takes to reach the target position, controlling the smoothness of the motion.</param>
        /// <returns>The new position after applying the smooth damp logic.</returns>
        public static Vector3 SmoothDamp(float deltaTime, Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime)
        {
            // Ensure smoothTime is not too small to avoid division by zero or instability.
            smoothTime = Math.Max(0.0001f, smoothTime);
            float omega = 2f / smoothTime; // Angular frequency of the spring-damper system.

            // Precompute exponential decay factor for the damping effect.
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

            // Calculate the difference between the current and target positions.
            Vector3 change = current - target;
            Vector3 originalTo = target; // Store the original target for later use.

            // Limit the maximum change to prevent overshooting or instability.
            float maxChange = smoothTime * deltaTime;
            Vector3 maxChangeVector = new Vector3(maxChange, maxChange, maxChange);
            change = Vector3.Clamp(change, -maxChangeVector, maxChangeVector);
            target = current - change; // Adjust the target to account for the clamped change.

            // Calculate the temporary velocity and update the current velocity.
            Vector3 temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;

            // Compute the new position based on the damped motion.
            Vector3 output = target + (change + temp) * exp;

            // If the new position overshoots the original target, snap to the target and reset velocity.
            if (Vector3.Dot(originalTo - current, output - originalTo) > 0)
            {
                output = originalTo;
                currentVelocity = Vector3.Zero;
            }

            return output; // Return the updated position.
        }

        /// <summary>
        /// Advanced smooth damping function that adapts speed based on distance to target.
        /// Accelerates when far from target and decelerates when approaching.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last frame in seconds</param>
        /// <param name="current">Current position</param>
        /// <param name="target">Target position</param>
        /// <param name="currentVelocity">Reference to current velocity (maintained between calls)</param>
        /// <param name="smoothTime">Base smooth time (smaller values = faster movement)</param>
        /// <param name="adaptiveFactorMin">Minimum acceleration factor (default: 0.5)</param>
        /// <param name="adaptiveFactorMax">Maximum acceleration factor (default: 2.0)</param>
        /// <param name="distanceThreshold">Distance at which max adaptation occurs (default: 10.0)</param>
        /// <returns>New position after applying adaptive smooth damping</returns>
        public static Vector3 SmoothDamp2(float deltaTime, Vector3 current, Vector3 target, ref Vector3 currentVelocity,
                                         float smoothTime, float adaptiveFactorMin = 0.5f, float adaptiveFactorMax = 2.0f,
                                         float distanceThreshold = 10.0f)
        {
            // Calculate distance to target
            float distance = Vector3.Distance(current, target);

            // Calculate adaptive smooth time - shorter when far, longer when close
            // Map distance [0, distanceThreshold] to smooth time factor [adaptiveFactorMin, adaptiveFactorMax]
            float distanceFactor = Math.Min(distance / distanceThreshold, 1.0f);
            float adaptiveFactor = adaptiveFactorMin + (adaptiveFactorMax - adaptiveFactorMin) * distanceFactor;

            // Apply adaptive factor to smooth time (invert since we want faster movement when far)
            float adaptiveSmoothTime = smoothTime / adaptiveFactor;

            // Ensure smoothTime is not too small to avoid division by zero or instability
            adaptiveSmoothTime = Math.Max(0.0001f, adaptiveSmoothTime);

            // Calculate angular frequency
            float omega = 2f / adaptiveSmoothTime;

            // Calculate exponential decay
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

            // Calculate position change
            Vector3 change = current - target;
            Vector3 originalTo = target;

            // Apply maximum change limit
            float maxChange = adaptiveSmoothTime * deltaTime;
            change = Vector3.Clamp(change, new Vector3(-maxChange), new Vector3(maxChange));
            target = current - change;

            // Calculate temporary velocity
            Vector3 temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;

            // Calculate output position
            Vector3 output = target + (change + temp) * exp;

            // Check if we've overshot the target
            if (Vector3.Dot(originalTo - current, output - originalTo) > 0)
            {
                output = originalTo;
                currentVelocity = Vector3.Zero;
            }

            return output;
        }

        /// <summary>
        /// A simplified smooth following function that ensures responsive movement even at large distances.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last frame in seconds</param>
        /// <param name="current">Current position</param>
        /// <param name="target">Target position</param>
        /// <param name="currentVelocity">Reference to current velocity vector (maintained between calls)</param>
        /// <param name="baseSpeed">Base movement speed in units per second</param>
        /// <param name="accelerationRate">How quickly speed increases with distance (higher = more aggressive)</param>
        /// <param name="dampingFactor">How quickly to dampen velocity (lower = smoother but slower to stop)</param>
        /// <returns>New position after applying movement</returns>
        public static Vector3 ResponsiveFollow(
            float deltaTime,
            Vector3 current,
            Vector3 target,
            ref Vector3 currentVelocity,
            float baseSpeed = 10.0f,
            float accelerationRate = 0.5f,
            float dampingFactor = 0.8f)
        {
            // Calculate direction and distance to target
            Vector3 toTarget = target - current;
            float distance = toTarget.Length();

            if (distance < 0.001f)
            {
                // Already at target, gradually stop
                currentVelocity *= dampingFactor;
                return current + currentVelocity * deltaTime;
            }

            // Normalize direction
            Vector3 direction = toTarget / distance;

            // Calculate speed based on distance (higher speed when further away)
            float targetSpeed = baseSpeed * (1.0f + distance * accelerationRate);

            // Calculate target velocity
            Vector3 targetVelocity = direction * targetSpeed;

            // Smoothly transition current velocity towards target velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, deltaTime * 3.0f);

            // Apply velocity to position
            return current + currentVelocity * deltaTime;
        }

        /// <summary>
        /// Extracts the rotation component from a transformation matrix.
        /// </summary>
        /// <param name="matrix">The transformation matrix to extract the rotation from.</param>
        /// <returns>A Quaternion representing the rotation component of the matrix.</returns>
        public static Quaternion ExtractRotation(Matrix matrix)
        {
            // Decompose the matrix into scale, rotation, and translation components.
            // Only the rotation component is returned.
            matrix.Decompose(out _, out Quaternion rotation, out _);
            return rotation;
        }
        /// <summary>
        /// Linearly interpolates a Vector3 value from the current position to the target position over time.
        /// The speed of movement is controlled by the inertia value.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
        /// <param name="current">The current position of the object.</param>
        /// <param name="target">The target position to move towards.</param>
        /// <param name="inertia">The speed factor controlling how quickly the object moves towards the target.</param>
        /// <returns>The new position after applying the linear interpolation.</returns>
        public static Vector3 SimpleLerp(float deltaTime, Vector3 current, Vector3 target, float inertia)
        {
            // Ensure inertia is not too small to avoid imperceptible movement
            inertia = Math.Max(0.01f, inertia);

            // Calculate the interpolation factor based on deltaTime and inertia
            float t = Math.Min(1.0f, deltaTime * inertia);

            // Linearly interpolate between the current and target positions
            return Vector3.Lerp(current, target, t);
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Clamp(t, 0f, 1f);
        }
    }
}
