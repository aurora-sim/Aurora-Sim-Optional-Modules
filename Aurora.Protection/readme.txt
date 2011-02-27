README

This module allows for you to ban users from your grid easily and have 
auto detection of other accounts that the user is trying to log on with as well.

--- Set up ---

Add the Aurora.Protection.dll file into the bin directory.
Add the Aurora.Protection.ini into
  -- if you are running standalone
     Configuration/Modules/
  -- if you are running in grid mode
     AuroraServerConfiguration/
    and add the following line to AuroraServer.ini
    include-auroraprotection = AuroraServerConfiguration/Aurora.Protection.ini

Configure the settings as you want them to be, and restart. It will then be working and ready to go.

-- Use ---

There are two console commands with this module,

UserInfo <UUID> OR UserInfo <First> <Last>
   This command shows info about the given user, such as the flags they have.
   
SetUserInfo <UUID> <Flags> OR SetUserInfo <First> <Last> <Flags>
   This commands sets the given flags for a user. If you wish to ban a user,
     set their Flags to Known and it will ban their account and any other alternates after 
     the next similarity check. If you wish to unban a user, set their Flags to Clean.
   The flags that are able to be set are "Clean", "Suspected", and "Known".
