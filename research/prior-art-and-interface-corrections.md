# QUEST-EAGLEEYE: verified interface corrections and research antecedents

Assessment: 21 October 2026. Status: engineering literature investigation, not measured project results.

## Significant correction: Quest passthrough versus external USB cameras

Meta's Passthrough Camera API (Horizon OS v74+, Quest 3/3S, camera permission, passthrough enabled) provides access to the **headset's forward-facing RGB cameras**. Its Camera2 and Unity texture interfaces do not establish that a TOPDON TC001 enumerates as a compatible camera or supplies frames through Unity `WebCamTexture`. Meta's official CameraViewer sample displays the Quest RGB camera, not external thermal imagery. The TC001 integration must be separately demonstrated through USB descriptors, host permissions, protocol/driver, decoder and operating-system behavior.

Sources:
- https://developers.meta.com/vr/documentation/unity/unity-pca-overview/ (updated October 2026)
- https://developers.meta.com/vr/documentation/unity/unity-sample-camera-viewer/
- https://developer.android.com/develop/connectivity/usb/host

## Registration is a distinct scientific problem

A flat thermal feed in world-space UI is NOT spatially registered thermal sensing. Rigid external-sensor calibration, camera intrinsics and distortion, frame timestamps, time-aligned Quest pose, depth/scene model, physical registration target and error measurements are necessary. Environment depth by itself does not determine thermal-to-RGB correspondence, and valid overlays at one distance do not imply alignment at other ranges.

Verified scholarly antecedents:
1. Chegini S, Edwards E, McGurk M, Clarkson M, Schilling C. *Systematic review of techniques used to validate the registration of augmented-reality images using a head-mounted device to navigate surgery*. British Journal of Oral and Maxillofacial Surgery. 2023;61(1):19–27. DOI: https://doi.org/10.1016/j.bjoms.2022.08.007. The review demonstrates established registration-validation methodologies in head-mounted AR.
2. *Design and evaluation of an AR-based thermal imaging system for planning reconstructive surgeries.* 2024. https://pubmed.ncbi.nlm.nih.gov/38789883/ . The study describes calibrated thermal augmentation, measured projection errors, and clinically insufficient alignment with the evaluated tracking approach. Verify full author list and DOI before manuscript bibliography finalization.
3. *Calibration of Augmented Reality Headset with External Tracking System Using AX=YB.* IEEE ISMAR 2024. DOI: https://doi.org/10.1109/ISMAR62088.2024.00035. Describes external tracking alignment and temporal-synchronization considerations.
4. Zhao C, Eom S, Beams R. *Spatiotemporal visualization of image registration on augmented reality head-mounted displays.* Optics Express. 2026;34(10):18125–18139. DOI: https://doi.org/10.1364/OE.584003. Demonstrates interaction between motion, rendering latency and geometric image registration.
5. *Dynamic registration correction in augmented-reality systems.* IEEE VRAIS 1995. DOI: https://doi.org/10.1109/VRAIS.1995.512495. Historical antecedent for dynamic image-registration correction.

These publications were validated at source-level for their metadata where specified, **not reproduced experimentally**. Their measured figures must not be transferred to QUEST-EAGLEEYE.

## Radio and positioning corrections

Meshtastic's Heltec V3 entry lists no integrated GPS. Position requires an actual GNSS-capable node, paired companion coordinates, or another traceable valid source. Correct processing needs WGS84 coordinate definition, local tangent reference origin, reference yaw, update age, reported accuracy, coordinate-transform implementation and packet-loss monitoring.

- https://meshtastic.org/docs/hardware/devices/heltec-automation/lora32/
- https://meshtastic.org/docs/
- https://heltec.org/project/wifi-lora-32-v3/

## USB-C and power caveat

`android.hardware.usb.host` and an Android manifest intent do not prove external device compatibility, thermal frame decoding, or Quest 3 charging while USB host. Confirm measured host role, VID/PID/interface/endpoint descriptors, decoded frames, Quest battery trend and thermal/power stability with exact hub, cable, sensor and headset firmware.

## Research boundaries and test requirements

The recommended first experiment is external Android/Windows thermal capture gateway to Quest 3 over a measured Wi-Fi link. Step 1: thermal 2D display; Step 2: separately calibrated world registration; Step 3: independently sourced and age-checked Meshtastic location display. Each stage requires distinct logs and pass/fail data. None has been demonstrated in this repository. No military operational equivalence, claimed performance, prototype completion, or scientific novelty is inferred.
