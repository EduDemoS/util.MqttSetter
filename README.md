# MQTT setter utility

Simple utility for resetting all the status values of a set of devices on the MQTT broker.
This can be used to get a clean starting point e.g. at the beginning of a workshop or to reset the dashboard rendering during the event.

At the moment, only sunflowers are supported. Reading device names from a file is not yet supported.

## How to use the utiity

1. Create a copy of `appsettings.default.json` and name it `appsettings.json`
2. Fill in the appropriate data for your setup
3. Run the utility

The utility will start sending out MQTT messages on behalf of various devices to the configured broker. 
Whenever you want to stop the process, simply press any key on the console window.

All data is licensed under the terms outlined in [LICENSE.md](LICENSE.md).
