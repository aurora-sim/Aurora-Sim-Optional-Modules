This module converts the old style asset and inventory .xml files to IARs so they can be used with the new IAR loaders in Aurora master. To run this tool, do the following.
1.) Compile this module into Aurora by dropping it into the addon-modules directory and running prebuild and then compiling.
2.) Copy over the .ini file included in this directory into the Configuration/Modules/ directory in the bin folder.
3.) Start Aurora.exe and it will create an IAR of the default assets and inventory.