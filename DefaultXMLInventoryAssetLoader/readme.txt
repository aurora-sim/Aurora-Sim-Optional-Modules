To import xml stuff to a clean/removed database (use mysql, so you can edit stuff straigh in the database later)
1. add this module to addon-modules and compile aurora-sim
2. add the contents of .ini file included in this module to DefaultInventory/Inventory.ini
3. change PreviouslyLoaded to false in Inventory.ini on the 2 xml chapters
4. change PreviouslyLoaded to true on InventoryIARLoader
4. copy assets and inventory folder in this module to DefaultInventory/
5. run Aurora.Server.exe to create the new database (automatically) and it imports the xml inventory
6. close aurora.server.exe
3. change PreviouslyLoaded to true in Inventory.ini on the 2 xml chapters
7. open mysql editor (heidisql for example) and goto the database of your grid
8. add library owner user account in the user account table
    "PrincipalID";"ScopeID";"FirstName";"LastName";"Email";"ServiceURLs";"Created";"UserLevel";"UserFlags";"UserTitle";"Name"
    "11111111-1111-0000-0000-000100bba000";"00000000-0000-0000-0000-000000000000";"Library";"Owner";"";"";"1326480106";"0";"0";"";"Library Owner"
9. add library root folder to inventoryfolders table
    "folderID";"agentID";"parentFolderID";"folderName";"type";"version"
    "00000112-000f-0000-0000-000100bba000";"11111111-1111-0000-0000-000100bba000";"00000000-0000-0000-0000-000000000000";"DefaultInventory/CorrectDefaultInventory.iar";"9";"1"

now you can continue to setup the grid like normal