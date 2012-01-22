In this directory you can place addon modules for Aurora

# Installation

To install, there are two ways to install, auto-installation, or manual compilation

## Automated
1. Start Aurora.exe or Aurora.Server.exe
2. Type 'compile module <path to the build.am of the module that you want>' into the console and it will install the module for you and tell you how to use or configure it.

## Manual Compilation and installation:
Each module should be in it's own tree and the root of the tree should contain a file named "prebuild.xml", which will be included in the main prebuild file.

The prebuild.xml should only contain <Project> and associated child tags. 
The <?xml>, <Prebuild>, <Solution> and <Configuration> tags should not be included since the add-on modules prebuild.xml will be inserted directly into the main prebuild.xml


# Known Issues
* MSSQL does not implement IndexDefinition
* TreePopulator won't load