# Development validation

This is an ongoing test record. Do not treat successful import or compilation
as evidence that the aircraft can take off or that its performance is authentic.

Verified so far:

- Pinned original source ZIP and its per-file SHA-256 manifest are retained.
- 31 source models convert and assemble in Unity 2022.3.62f2.
- The experimental Blueprinter bundle compiles and loads in Nuclear Option
  0.34.2. The aircraft has its own silhouette, rank 1 and $13 million purchase
  price in the native hangar.
- A fresh in-game aircraft was created in the local no-hostiles mission. The
  source cabin and native HUD rendered, and all three wheel assemblies reported
  ground contact. Both engines reported 12,680 N configured maximum dry thrust
  and no afterburner. This first candidate is not a passing flight build.

Issues found and corrected in code, awaiting a fresh flight test:

- The support plugin initially patched a nonexistent method. The target is now
  the native Aircraft.SetComplexPhysics method.
- Empty mass initially excluded 225 kg incorrectly; the target is now 2,817 kg.
- OBJ import mirrors the intermediate X axis. XML offsets and animation
  rotations must use the imported geometry's handedness.
- Native JetNozzle.thrustTransform can refer to the nozzle AeroPart itself.
  Moving that transform also moved its tail and elevator children. Dedicated
  thrust-point children now prevent this hierarchy side effect.
- Native hull adaptation is audited for penetration between unlinked parts.
  No collision-ignore patch, invulnerability or artificial strengthening is used.

Still required: a new spawn with the corrected support plugin, taxi/takeoff,
gear cycles, camera switching, acceleration/deceleration, control response,
weapons separation/firing, landing/braking and repeat health inspection.
FAU markings, functional analog instruments and internal gun remain unfinished.

Tests/A37TestProbe.cs supplies temporary control inputs and telemetry for the
local test mission. Its placed-runway/approach fixtures must be identified as
fixtures; subsequent flight uses native forces. Remove its DLL before release.
