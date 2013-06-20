<?php
defined('_JEXEC') or die('Direct Access to this location is not allowed.');
 
class ModStarDust_PurchaseHelper
{

	function IPNValidation($STARDUST_SERVICE_URL, $NOTIFICATION_EMAIL, $DO_NOTIFICATION)
	{
		$returnValue = array();
		$returnValue[0] = "1";
		$req = "";
		
		foreach ($_POST as $key => $value) 
		{
			$value = urlencode(stripslashes($value));
			$req .= "&$key=$value";
		}
		
		$aconfig =  ModStarDust_PurchaseHelper::GetConfigSettings();
		$found = array();
		$found[0] = json_encode(array_merge (array('Method' => 'IPNData', 'WebPassword' => md5($aconfig['webui_password']), 'req' => $req), $_POST));
		$do_post_request = ModStarDust_PurchaseHelper::do_post_request($found, $STARDUST_SERVICE_URL);
		$recieved = json_decode($do_post_request);

		if ($recieved->{'Verified'} == "True") 
		{
			if ($DO_NOTIFICATION == "1")
			{
				ModStarDust_PurchaseHelper::SendEmailIPN($NOTIFICATION_EMAIL, $req, $do_post_request);
			}
		}
		else
		{
			ModStarDust_PurchaseHelper::SendErrorEmailIPN($NOTIFICATION_EMAIL, $req, $do_post_request);
		}
		return $recieved;
	}


	function checkItems($STARDUST_SERVICE_URL, $tx, $st, $amt, $cc, $cm, $item_number, $req, $NOTIFICATION_EMAIL, $DO_NOTIFICATION)
	{
		$returnValue = array();
		$returnValue[0] = "1";
		
		$aconfig =  ModStarDust_PurchaseHelper::GetConfigSettings();
		$found = array();
		$found[0] = json_encode(array('Method' => 'Validate', 'WebPassword' => md5($aconfig['webui_password']), 'tx' => $tx));
		$do_post_request = ModStarDust_PurchaseHelper::do_post_request($found, $STARDUST_SERVICE_URL);
		$recieved = json_decode($do_post_request);
		
		$isDoneAlready = ($recieved->{'CompleteType'} == "ALREADYDONE");
		if ($recieved->{'Verified'} == true) 
		{
			try
			{
				$returnValue[0] = "2";
				$returnValue[1] = $recieved;
				if (($DO_NOTIFICATION == "1") && (!$isDoneAlready))
				{
					ModStarDust_PurchaseHelper::SendEmail($STARDUST_SERVICE_URL, $tx, $st, $amt, $cc, $cm, $item_number, $req, $NOTIFICATION_EMAIL, $DO_NOTIFICATION, $do_post_request);
				}
			} 
			catch (Exception $e) 
			{
				echo '<pre>Exception:';
				var_dump($e);
				echo '</pre>';
			}
		}
		else
		{
			ModStarDust_PurchaseHelper::SendErrorEmail($STARDUST_SERVICE_URL, $tx, $st, $amt, $cc, $cm, $item_number, $req, $NOTIFICATION_EMAIL, $DO_NOTIFICATION, $do_post_request);
		}
		return $returnValue;
	}
	
	function SendEmail($STARDUST_SERVICE_URL, $tx, $st, $amt, $cc, $cm, $item_number, $req, $NOTIFICATION_EMAIL, $DO_NOTIFICATION, $recieved)
	{
		try
		{
			$mailer =& JFactory::getMailer();
			$config =& JFactory::getConfig();
			$sender = array( 
				$config->getValue( 'config.mailfrom' ),
				$config->getValue( 'config.fromname' ) );
			$mailer->setSender($sender);
			$mailer->addRecipient($NOTIFICATION_EMAIL);
			$body = JText::sprintf(PURCHASE_NOTICE, $amt, $st, $cc, $cm, $tx, $recieved);
			$mailer->setSubject('Stardust - Purchase');
			$mailer->setBody($body);
			$send =& $mailer->Send();
		} 
		catch (Exception $e) 
		{
			echo '<pre>Exception:';
			var_dump($e);
			echo '</pre>';
		}
	}
	
	function SendErrorEmail($STARDUST_SERVICE_URL, $tx, $st, $amt, $cc, $cm, $item_number, $req, $NOTIFICATION_EMAIL, $DO_NOTIFICATION, $recieved)
	{
		try
		{
			$mailer =& JFactory::getMailer();
			$config =& JFactory::getConfig();
			
			$sender = array( 
				$config->getValue( 'config.mailfrom' ),
				$config->getValue( 'config.fromname' ) );
			 
			$mailer->setSender($sender);
			$mailer->addRecipient($NOTIFICATION_EMAIL);
			$body = JText::sprintf(ERROR_EMAIL, $amt, $st, $cc, $cm, $tx, $recieved);
			$mailer->setSubject('Stardust - Error');
			$mailer->setBody($body);
			$send =& $mailer->Send();
		} catch (Exception $e) {
			echo '<pre>Exception:';
			var_dump($e);
			echo '</pre>';
		}
	}
	
	function SendEmailIPN($NOTIFICATION_EMAIL, $recievedPost, $recieved)
	{
		try
		{
			$mailer =& JFactory::getMailer();
			$config =& JFactory::getConfig();
			$sender = array( 
				$config->getValue( 'config.mailfrom' ),
				$config->getValue( 'config.fromname' ) );
			$mailer->setSender($sender);
			$mailer->addRecipient($NOTIFICATION_EMAIL);
			$body = JText::sprintf(IPN_EMAIL, $recievedPost, $recieved);
			$mailer->setSubject('Stardust - IPN');
			$mailer->setBody($body);
			$send =& $mailer->Send();
		} 
		catch (Exception $e) 
		{
			echo '<pre>Exception:';
			var_dump($e);
			echo '</pre>';
		}
	}
	
	function SendErrorEmailIPN($NOTIFICATION_EMAIL, $recievedPost, $recieved)
	{
		try
		{
			$mailer =& JFactory::getMailer();
			$config =& JFactory::getConfig();
			$sender = array( 
				$config->getValue( 'config.mailfrom' ),
				$config->getValue( 'config.fromname' ) );
			$mailer->setSender($sender);
			$mailer->addRecipient($NOTIFICATION_EMAIL);
			$body = JText::sprintf(IPN_EMAIL_ERROR, $recievedPost, $recieved);
			$mailer->setSubject('Stardust - IPN Error');
			$mailer->setBody($body);
			$send =& $mailer->Send();
		} 
		catch (Exception $e) 
		{
			echo '<pre>Exception:';
			var_dump($e);
			echo '</pre>';
		}
	}
	
	function getItems($user, $STARDUST_SERVICE_URL)
	{
		$aconfig =  ModStarDust_PurchaseHelper::GetConfigSettings();
		$UUID = ModStarDust_PurchaseHelper::GetUserUUID($user);
		$returnValue = array();
		$returnValue[0] = "1";
		if ($UUID)
		{
			$_SESSION['USERIDUUID'] = $UUID;
			$found = array();

			$found[0] = json_encode(array('Method' => 'CheckPurchaseStatus', 'WebPassword' => md5($aconfig['webui_password']), 'purchase_id' => $_SESSION['purchase_id'], 'principalId' => $UUID));

			$do_post_request = ModStarDust_PurchaseHelper::do_post_request($found, $STARDUST_SERVICE_URL);

			$recieved = json_decode($do_post_request);
			
			// echo '<pre>';
			// var_dump($_SESSION['PAYPAL_URL']);
			// var_dump($recieved);
			// var_dump($do_post_requested);
			// echo("****".$recieved->{'FailNumber'}."#*********");
			// echo '</pre>';
			
			if ($recieved->{'FailNumber'} != "0") 
			{
				if ($recieved->{'Reason'} != "")
				{
					echo($recieved->{'Reason'});
					if ($recieved->{'FailNumber'} == 2)
					{
						echo '</br>'.
						'<b>How to fix this:</b><br/>'.
						'1) Please logout<br/>'.
						'2) Log back in as the correct user<br/>'.
						'3) Then re-open the URL given to you in your local chat window.<br/>';
					}
				}
				else
				{
					echo("Unknown Issues.. Please try again in a bit. Sorry :(");
				}
			}
			else
			{
				$returnValue[0] = "0";
				$returnValue[1] = $recieved->{'USDAmount'} / 100;
				if ($recieved->{'PurchaseType'} == "1")
				{
					$returnValue[2] = "Currency Purchase of ".$recieved->{'Amount'};
					$returnValue[3] = "_xclick";
				}
				else
				{
					$returnValue[2] = $recieved->{'RegionName'} . " Region";
					$returnValue[3] = "_xclick-subscriptions";
				}
				$returnValue[4] = $recieved;
			}
			
		}
		return $returnValue;
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
		$aconfigSettings = array();
		$configCount = count($configSettings);
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