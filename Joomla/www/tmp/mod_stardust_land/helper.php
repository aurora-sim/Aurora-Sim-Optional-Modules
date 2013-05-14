<?php
defined('_JEXEC') or die('Direct Access to this location is not allowed.');
 
class ModStarDust_LandHelper
{

	function SendToPayPal($user, $PAYPAL_URL,$NOTIFY_URL,$PAYPAL_ACCOUNT,$RETURN_URL,$DO_NOTIFICATION,$NOTIFICATION_EMAIL,$STARDUST_SERVICE_URL)
	{
		$session =& JFactory::getSession();
		$aconfig = ModStarDust_LandHelper::GetConfigSettings();
		
		$UUID = ModStarDust_LandHelper::GetUserUUID($user);
		$found = array();
		$found[0] = json_encode(array('Method' => 'OrderSubscription', 'WebPassword' => md5($aconfig['webui_password']), 'toId' => $UUID, 'regionName' => $_POST["name"], 'notes' => $_POST["notes"], 'subscription_id' => $_POST["idx"]));
		$do_post_request = ModStarDust_LandHelper::do_post_request($found, $STARDUST_SERVICE_URL);
		$recieved = json_decode($do_post_request);
		
		// echo '<pre>';
		// var_dump($UUID);
		// var_dump($recieved);
		// var_dump($do_post_requested);
		// echo '</pre>';
		
		if ($recieved->{'Verified'} == "true") 
		{
			$productInfo = ModStarDust_LandHelper::GetProductInfo($aconfig );
			$pid = $recieved->{'purchaseID'};
			$session->set('paypalAmount', $productInfo["price"]);
			$session->set('purchase_id', $pid);
			$session->set('paypalPurchaseItem', $productInfo["name"]);
			
			header("Location: /send_to_paypal.php");
			return "";
		}
		else
		{
			if ($recieved->{'Reason'} != "")
			{
				return $recieved->{'Reason'};
			}
			else
			{
				return "Unknown Error. Please try again in a bit.";
			}
		}
	}

	function ValidateForm()
	{
		if (empty($_POST["agree"]))
		{
			return "You must agree with the Terms Of Service";
		}
		
		if (empty($_POST["name"]))
		{
			return "You must name your Island";
		}
		else
		{
			return ModStarDust_LandHelper::CheckSimExist();
		}
	}
	
	function CheckSimExist()
	{
		$aconfig = ModStarDust_LandHelper::GetConfigSettings();
		$options = array();
		$option['driver'] 	= $aconfig['aurora_database_type'];
		$option['host']     = $aconfig['aurora_database_host'];
		$option['database'] = $aconfig['aurora_database_name'];
		$option['user']     = $aconfig['aurora_database_user'];
		$option['password'] = $aconfig['aurora_database_pass'];
		$count = ModStarDust_LandHelper::CheckSimExist1($option) + ModStarDust_LandHelper::CheckSimExist2($option);
		if ($count == 0) return "";
		else return "This region name is already in use.";
	}
	
	function CheckSimExist1($option)
	{
		$db = &JDatabase::getInstance( $option );
		$query = "SELECT count(*) FROM gridregions WHERE RegionName = '".$_POST[name]."'";
		$db->setQuery( $query );
		$result = $db->loadResult();
		return $result;
	}
	
	function CheckSimExist2($option)
	{
		$db = &JDatabase::getInstance( $option );
		$query = "SELECT count(*) FROM stardust_purchased WHERE RegionName = '".$_POST[name]."' AND Complete = 1";
		$db->setQuery( $query );
		$result = $db->loadResult();
		return $result;
	}
	
	function GetProductInfo($aconfig)
	{
		$options = array();
		$option['driver'] 	= $aconfig['aurora_database_type'];
		$option['host']     = $aconfig['aurora_database_host'];
		$option['database'] = $aconfig['aurora_database_name'];
		$option['user']     = $aconfig['aurora_database_user'];
		$option['password'] = $aconfig['aurora_database_pass'];
		
		$db = &JDatabase::getInstance( $option );
		$query = "SELECT name, price FROM stardust_subscriptions WHERE id = '".$_POST["idx"]."' AND active = 1";
		$db->setQuery( $query );
		$results = $db->loadRow();
		$results2= array();
		$results2["name"] = $results[0];
		$results2["price"] = $results[1] / 100.0;
		return $results2;
	}
	
	function GetItemsForSale()
	{
		$aconfig = ModStarDust_LandHelper::GetConfigSettings();
		$options = array();
		$option['driver'] 	= $aconfig['aurora_database_type'];
		$option['host']     = $aconfig['aurora_database_host'];
		$option['database'] = $aconfig['aurora_database_name'];
		$option['user']     = $aconfig['aurora_database_user'];
		$option['password'] = $aconfig['aurora_database_pass'];
		$db = &JDatabase::getInstance( $option );

		$sql = "SELECT `id`, `name`, `description`, `price`, `active` FROM `stardust_subscriptions` WHERE `active` = 1";
		$db->setQuery( $sql );
		$items = ($items = $db->loadAssocList())?$items:array();
		$returnValue = array();
		$returnValue[0] = $items;
		return $items;
	}

	function do_post_request($found, $STARDUST_SERVICE_URL) 
	{
		$params = array('http' => array(
				'method' => 'POST',
				'content' => implode(',', $found)
				));
		$ctx = stream_context_create($params);
		$timeout = 15;
		$old = ini_set('default_socket_timeout', $timeout);
		$fp = @fopen($STARDUST_SERVICE_URL, 'rb', false, $ctx);
		ini_set('default_socket_timeout', $old);
		if ($fp) {
			stream_set_timeout($fp, $timeout);
			stream_set_blocking($fp, 3);
		} else{
			if ($fp) fclose($fp);
			return false;
		}
		$response = @stream_get_contents($fp);
		if ($fp) fclose($fp);
		return $response;
	}
	
	function GetUserUUID($user)
	{
		$db =& JFactory::getDBO();
		$query = 'SELECT `uuid`'
			. ' FROM #__aurorasim_user'
			. ' WHERE joomla_userid=' . $user->id;
		$db->setQuery( $query );
		$result = $db->loadResult();
		return $result;
	}
	
	function GetConfigSettings()
	{	
		@$aconfigSettings = array();
		$configCount = count($aconfigSettings);
		if ($configCount == 0)
		{
			$db =& JFactory::getDBO();
			$query = 'SELECT `webui_gridname`, `webui_url`, `webui_texture_url`, `webui_password`, `isdefault`, `aurora_database_type`, `aurora_database_host`, `aurora_database_name`, `aurora_database_user`, `aurora_database_pass` '
				. ' FROM #__aurorasim'
				. ' WHERE isdefault = 1' ;
			$db->setQuery( $query );
			$results = $db->loadRow();
			
			$aconfigSettings['webui_gridname'] = $results['0'];
			$aconfigSettings['webui_url'] = $results['1'];
			$aconfigSettings['webui_texture_url'] = $results['2'];
			$aconfigSettings['webui_password'] = $results['3'];
			$aconfigSettings['isdefault'] = $results['4'];
			$aconfigSettings['aurora_database_type'] = $results['5'];
			$aconfigSettings['aurora_database_host'] = $results['6'];
			$aconfigSettings['aurora_database_name'] = $results['7'];
			$aconfigSettings['aurora_database_user'] = $results['8'];
			$aconfigSettings['aurora_database_pass'] = $results['9'];
		}
		// echo '<pre>';
		// var_dump($aconfigSettings);
		// var_dump($results);
		// echo '</pre>';
		// throw new Exception("Problem with nothing.. just testing");
		return $aconfigSettings;
	}
} //end ModHelloWorld2Helper
?>