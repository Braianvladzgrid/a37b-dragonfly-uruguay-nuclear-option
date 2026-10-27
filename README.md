# A-37B Dragonfly Uruguay 0.1.0 Beta

Playable experimental adaptation for Nuclear Option0.34.2 and Blueprinter2.0.1.
Tier1, price$13million. Install the nobp and A37FlightSupport.dll together in
BepInEx/plugins/com.nikkorap.blueprinter/addons/vladzgrid.a37/ and create a new
plane. Never install the local A37TestProbe.dll.

Includes two dry turbojets without afterburner, original cockpit/seats and two
decorative crew, calibrated analog instruments, native functional HUD, dedicated
catalog/map silhouette, FAU282 woodland livery/Artigas finflash, animated native
landing gear and source doors, eight external supports and internal7.62mm gun.

Native-force tests passed takeoff, near-base approach/touchdown/full stop,
retraction/extension, camera switching, gun firing and mixed bomb/rocket release.
Clean sustained speed measured795.6km/h at1690.8m, compared with FAU reference
770km/h. This is an engineering adaptation, not an authenticated flight model.
See TESTING.md and References/PERFORMANCE.md for conditions and fixture limits.

Known limitations: number stencils partially clip into the curved fuselage;
HUD aircraft outline/avionics still come from the native donor; original engine
RPM/EGT instruments have no telemetry bridge; native compact backup displays
remain. Controller orbit sensitivity, full manual handling, multiplayer and
all combinations of third-party weapons are not certified. Test relocation
from a distant/high-speed flight can break native joints; that fixture is not
included in this addon. No invulnerability, velocity clamp or collision bypass.

Model: FlightGear FGAddon revision21797, Emmanuel Baranger/helijah, GPL2, with
pilot attribution to Bertrand Augras. Original archive and corresponding model
source are included in this repository. Integration/editor/runtime codeMIT.
See CREDITS.md and SourceAssets/COPYING-GPL2.txt.

Build with Unity2022.3.62f2 and Blueprinter2.0.1; import the installed game for
local placeholders/assemblies (excluded from Git). Run
A37VisualBuilder.PrepareFlight without -quit; its domain-reload continuation
finishes the build. RuntimeSupport/compile-runtime.ps1 builds the support DLL.
