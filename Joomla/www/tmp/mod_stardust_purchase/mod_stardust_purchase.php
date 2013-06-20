<?php
//no direct access
defined('_JEXEC') or die('Direct Access to this location is not allowed.');
 
// include the helper file
require_once(dirname(__FILE__).DS.'helper.php');
$session =& JFactory::getSession();

$DO_NOTIFICATION = $params->get('DO_NOTIFICATION');
$NOTIFICATION_EMAIL = $params->get('NOTIFICATION_EMAIL');
$STARDUST_SERVICE_URL = $params->get('STARDUST_SERVICE_URL');
$HOW_TO_BUY_LINK = $params->get('HOWTOBUY_DOC_LINK');
$DO_REDIRECT = $params->get('DO_REDIRECT');

if (count($_POST) >= 1)
{
	$results = ModStarDust_PurchaseHelper::IPNValidation($STARDUST_SERVICE_URL, $NOTIFICATION_EMAIL, $DO_NOTIFICATION);
	if ($recieved->{'Verified'} == "True")
	{
		// this was a call from paypal to let us know about its status
		exit(); 
	}			
}

$user =& JFactory::getUser();
$tx_token = $_GET['tx'];

if ($tx_token != "")
{
	$tx = $_GET[tx];
	$st = $_GET[st];
	$amt = $_GET[amt];
	$cc = $_GET[cc];
	$cm = $_GET[cm];
	$item_number = $_GET[item_number];
	$req = 'cmd=_notify-synch';
	$results = ModStarDust_PurchaseHelper::checkItems($STARDUST_SERVICE_URL, $tx, $st, $amt, $cc, $cm, $item_number, $req, $NOTIFICATION_EMAIL, $DO_NOTIFICATION);
	$waserror = $results[0];
	$recieved = $results[1];
	// clear all session values
	$_SESSION['purchase_id'] = "";
	$session->set('purchase_id', "");
	$session->set('PAYPAL_URL', "");
	$session->set('NOTIFY_URL', "");
	$session->set('PAYPAL_ACCOUNT', "");
	$session->set('RETURN_URL', "");
	$session->set('USERIDUUID', "");
	$session->set('paypalAmount', "");
	$session->set('paypalPurchaseItem', "");
	$session->set('purchase_type', "");
	require(JModuleHelper::getLayoutPath('mod_stardust_purchase'));
}
else
{
	if ($user->guest) {
		$loginURL = 'index.php?option=com_user&view=login';
		$myurl = $_SERVER['REQUEST_URI'];
		$myurl = base64_encode($myurl);
		$myurl = '&return='.$myurl;
		header("Location: ".$loginURL.$myurl);
	} else { 
		$AmountAdditionPerfectage = $params->get('AmountAdditionPerfectage');
		
		if ($_GET['purchase_id'] != "")
		{
			$session->set('purchase_id', $_GET['purchase_id']);
			$_SESSION['purchase_id'] = $_GET['purchase_id'];
		}
		
		if ($_SESSION['purchase_id'] != '')
		{
			$PAYPAL_URL = $params->get('PAYPAL_URL');
			$NOTIFY_URL = $params->get('NOTIFY_URL');
			$PAYPAL_ACCOUNT = $params->get('PAYPAL_ACCOUNT');
			$RETURN_URL = $params->get('RETURN_URL');
			
			$session->set('PAYPAL_URL', $PAYPAL_URL);
			$session->set('NOTIFY_URL', $NOTIFY_URL);
			$session->set('PAYPAL_ACCOUNT', $PAYPAL_ACCOUNT);
			$session->set('RETURN_URL', $RETURN_URL);
			$session->set('USERIDUUID', $_SESSION['USERIDUUID']);
			
			
			// get the items to display from the helper
			$results = ModStarDust_PurchaseHelper::getItems($user, $STARDUST_SERVICE_URL);
			$waserror = $results[0];
			if ($waserror == "0")
			{
				$paypalAmount = $results[1];
				$paypalPurchaseItem = $results[2];
				$purchase_type = $results[3];
				$recieved = $results[4];
				
				$session->set('paypalAmount', $paypalAmount);
				$session->set('paypalPurchaseItem', $paypalPurchaseItem);
				$session->set('purchase_type', $purchase_type);
				
				require(JModuleHelper::getLayoutPath('mod_stardust_purchase'));
			}
		}
		else
		{
		
			if ($DO_REDIRECT == "1")
				header("Location: ".$HOW_TO_BUY_LINK);
			else
				require(JModuleHelper::getLayoutPath('mod_stardust_purchase'));
		}
	}	
}
?>