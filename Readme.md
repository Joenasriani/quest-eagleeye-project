Project EagleEye
​Low-Cost COTS-Based Augmented Reality Situational Awareness System
​Author: QuestRequestVR (Joe Nasr)
Date: December 10, 2025
Type: Technical Research Paper

​Overview

​Project EagleEye is an academic research initiative designed to democratize tactical situational awareness. It replicates the capabilities of advanced military Integrated Visual Augmentation Systems (IVAS) using accessible Consumer Off-The-Shelf (COTS) hardware.
​By leveraging the "Passthrough" API of the Meta Quest 3, this system overlays thermal imaging data and LoRa-based team positioning (via Meshtastic) directly onto the user's real-world view.

​Repository Contents

​This repository hosts the full academic research paper and technical documentation as a web presentation (index.html). It includes:
​System Architecture: Wiring schematics for USB-C Power Delivery and data topology.
​Hardware Implementation: A complete Bill of Materials (BOM) costing approximately 3,321 AED (~3% of industry standard costs).
​Software Code: - Android Manifest overrides for USB Host permissions.
​Unity C# scripts for Thermal Camera integration.
​Gaze-based interaction scripts for hands-free control.
​Field Validation: Real-world testing data regarding latency, range, and battery endurance.

​Key Hardware Stack

​Compute: Meta Quest 3 (Android-based MR)
​Communications: Heltec V3 (LoRa Mesh Radio)
​Sensors: Topdon TC001 (LWIR Thermal Imager)
​Power/Data: Active USB-C PD Hub + 65W Battery Bank
​
Usage
​View Online
>> https://joenasriani.github.io/quest-eyeeagle-project
