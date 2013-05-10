<?php
//no direct access
defined('_JEXEC') or die('Direct Access to this location is not allowed.');
 
// include the helper file
require_once(dirname(__FILE__).DS.'helper.php');

if ($_GET['x'] && $_GET['y'])
{
	
}
else
{
	// get the items to display from the helper
	$results = ModAurora_List_RegionsHelper::getItems();
	 
	$count = $results[0];
	$AStart = $results[1];
	$ALimit = $results[2];
	$sitemax = $results[3];
	$sitestart = $results[4];
	$items = $results[5];
	 
	// include the template for display
	require(JModuleHelper::getLayoutPath('mod_aurora_list_regions'));
}
 

?>