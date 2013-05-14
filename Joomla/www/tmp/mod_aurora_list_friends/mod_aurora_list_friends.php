<?php
//no direct access
defined('_JEXEC') or die('Direct Access to this location is not allowed.');
 
// include the helper file
require_once(dirname(__FILE__).DS.'helper.php');

$user =& JFactory::getUser();
if (!$user->guest) 
{
	$results = ModAurora_List_FriendsHelper::getItems($user);
	$items = $results[0];
	$textureServer = $results[1];
	// include the template for display
	require(JModuleHelper::getLayoutPath('mod_aurora_list_friends'));
}
else
{
	// include the template for display
	require(JModuleHelper::getLayoutPath('mod_aurora_list_friends'));
}



 

?>