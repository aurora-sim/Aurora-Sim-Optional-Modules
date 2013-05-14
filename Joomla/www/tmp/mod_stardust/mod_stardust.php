<?php
//no direct access
defined('_JEXEC') or die('Direct Access to this location is not allowed.');
 
// include the helper file
require_once(dirname(__FILE__).DS.'helper.php');
 
 
	$user =& JFactory::getUser();
	
	if ($user->guest) {
		$loginURL = 'index.php?option=com_user&view=login';
		$myurl = $_SERVER['REQUEST_URI'];
		$myurl = base64_encode($myurl);
		$myurl = '&return='.$myurl;
		header("Location: ".$loginURL.$myurl);
	} else { 
		// get the items to display from the helper
		$results = ModStarDustHelper::getItems($user);

		$count = $results[0];
		$AStart = $results[1];
		$ALimit = $results[2];
		$UUID = $results[3];
		$sitemax = $results[4];
		$sitestart = $results[5];
		$items = $results[6];

		// include the template for display
		require(JModuleHelper::getLayoutPath('mod_stardust'));
	}
?>