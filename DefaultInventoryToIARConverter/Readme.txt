This module converts the old style asset and inventory .xml files to IARs so they can be used with the new IAR loaders in Aurora master. To run this tool, do the following.
1.) Compile this module into Aurora by dropping it into the addon-modules directory and running prebuild and then compiling.
2.) Copy over the .ini contents included in this directory into the DefaultInventory/Inventory.ini file.
3.) Start Aurora.Server.exe and it will create an IAR of the default assets and inventory, in the bin folder.