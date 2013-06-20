<?php
/**
 * @version		$Id: aurora.php 14401 2010-01-26 14:10:00Z louis $
 * @package		Joomla
 * @subpackage	JFramework
 * @copyright	Copyright (C) 2005 - 2010 Open Source Matters. All rights reserved.
 * @license		GNU/GPL, see LICENSE.php
 * Joomla! is free software. This version may have been modified pursuant
 * to the GNU General Public License, and as distributed it includes or
 * is derivative of works licensed under the GNU General Public License or
 * other free or open source software licenses.
 * See COPYRIGHT.php for copyright notices and details.
 */

// Check to ensure this file is included in Joomla!
defined('_JEXEC') or die( 'Restricted access' );

jimport('joomla.plugin.plugin');
jimport('joomla.application.component.helper'); // include libraries/application/component/helper.php

/**
 * Aurora User Plugin
 *
 * @package		Joomla
 * @subpackage	JFramework
 * @since 		1.5
 */
class plgUserAurora extends JPlugin {

	var $currentUser = Array('username' => '', 'block' => -1, 'password' => '', 'activation' => '', 'password_clear' => '');
	var $configSettings = Array();
	
	
	/**
	 * Constructor
	 *
	 * For php4 compatability we must not use the __constructor as a constructor for plugins
	 * because func_get_args ( void ) returns a copy of all passed arguments NOT references.
	 * This causes problems with cross-referencing necessary for the observer design pattern.
	 *
	 * @param object $subject The object to observe
	 * @param 	array  $config  An array that holds the plugin configuration
	 * @since 1.5
	 */
	function plgUserAurora(& $subject, $config)
	{
		parent::__construct($subject, $config);
		
	}

	/**
	 * Aurora store user method
	 *
	 * Method is called before user data is stored in the database
	 *
	 * @param 	array		holds the old user data
	 * @param 	boolean		true if a new user is stored
	 */
	function onBeforeStoreUser($user, $isnew)
	{
		global $currentUser;
		if ($isnew)
		{
		
		}
		else
		{
			if ($user["username"])
			{
				$this->currentUser['username'] = $user["username"];
			}
			if ($user["block"])
			{
				$this->currentUser["block"] = $user["block"];
			}
			if ($user["password"])
			{
				$this->currentUser["password"] = $user["password"];
			}
			if ($user["activation"])
			{
				$this->currentUser["activation"] = $user["activation"];
			}
			if ($user["password_clear"])
			{
				$this->currentUser["password_clear"] = $user["password_clear"];
			}
		}
		
		// echo '<pre>';
		// var_dump($this->currentUser);
		// echo '</pre>';
		// throw new Exception("Problem with nothing.. just testing");
		
	}

	/**
	 * Aurora store user method
	 *
	 * Method is called after user data is stored in the database
	 *
	 * @param 	array		holds the new user data
	 * @param 	boolean		true if a new user is stored
	 * @param	boolean		true if user was succesfully stored in the database
	 * @param	string		message
	 */
	function onAfterStoreUser($user, $isnew, $success, $msg)
	{
		global $mainframe;
		global $currentUser;
		
		if ($success)
		{		
			if ($isnew)
			{
				// we have to create the user now, because, this is the only time we have their password.
				// I was going to wait till after activation.. but.. you loose the password then.. 
				$this->CreateAuroraUser($user);
			}
			else
			{
				$usersParams = &JComponentHelper::getParams( 'com_users' );
				$useractivation = $usersParams->get( 'useractivation' ); // in this example, we load the config-setting
				if ($useractivation == 1)
				{
					if ($user["block"] == 0)
					{
						$this->ChangeActivation($user, 0);
					}
					else if ($user["block"] == 1)
					{
						$this->ChangeActivation($user, -1);
					}
				}
			}
			
			if (!$isnew)
			{
				if (!empty($_POST['password'])) 
				{
					
					if ($_POST['password'] == $_POST['password2'])
					{
						$result = $this->GetUserUUID($user);

						if ($result) 
						{
							$this->ChangePassword($result, $_POST['password']);
						}
					}
				}
				
				if (!empty($_POST['password1'])) 
				{
					
					if ($_POST['password1'] == $_POST['password2'])
					{
						$result = $this->GetUserUUID($user);
						if ($result) 
						{
							$this->ChangePassword($result, $_POST['password1']);
						}
					}
				}
			}
		}
	}
	
	function GetConfigSettings()
	{	
		global $configSettings;
		$configCount = count($configSettings);
		if ($configCount == 0)
		{
			$db =& JFactory::getDBO();
			$query = 'SELECT `webui_gridname`, `webui_url`, `webui_texture_url`, `webui_password`, `isdefault`'
				. ' FROM #__aurorasim'
				. ' WHERE isdefault = 1' ;
			$db->setQuery( $query );
			$results = $db->loadRow();
			
			$this->configSettings['webui_gridname'] = $results['0'];
			$this->configSettings['webui_url'] = $results['1'];
			$this->configSettings['webui_texture_url'] = $results['2'];
			$this->configSettings['webui_password'] = $results['3'];
			$this->configSettings['isdefault'] = $results['4'];
		}

		return $this->configSettings;
	}
	
	function ChangePassword($uuid, $password)
	{
		$aconfig = $this->GetConfigSettings();
		$found = array();
		$found[0] = json_encode(array('Method' => 'ForgotPassword', 'WebPassword' => md5($aconfig['webui_password']), 'UUID' => $uuid, 'Password' => $password));
		$do_post_requested = $this->do_post_request($found);
		$recieved = json_decode($do_post_requested);
		$returnValue = $recieved->{'Verified'} == 1;
		return $returnValue;
	}
	
	function ChangeActivation($user, $value)
	{
		global $currentUser;
		
		$aconfig = $this->GetConfigSettings();

		$result = $this->GetUserUUID($user);

		if ($result) 
		{
			$found = array();
			$found[0] = json_encode(array('Method' => 'Authenticated', 'WebPassword' => md5($aconfig['webui_password']), 'UUID' => $result, 'Verified' => $value));
			
			$do_post_requested = $this->do_post_request($found);
			$recieved = json_decode($do_post_requested);
			
			$returnValue = $recieved->{'Verified'} == 1;	
		}
		return $returnValue;
	}
	
	function CheckAuroraUserExists($user)
	{
		global $currentUser;
		
		$aconfig = $this->GetConfigSettings();
		$thisusername = $user["username"];
		$found = array();
		
		$found[0] = json_encode(array('Method' => 'CheckIfUserExists', 'WebPassword' => md5($aconfig['webui_password']), 'Name' => $thisusername));
		
		$do_post_requested = $this->do_post_request($found);
		$recieved = json_decode($do_post_requested);

		$returnValue = $recieved->{'Verified'} == 1;	
		if ($returnValue)
		{
			$this->currentUser["uuid"] = $recieved->{'UUID'};
		}
		return $returnValue;
	}
	
	function CreateAuroraJoomlaLink($uuid, $joomlaid)
	{
		$db =& JFactory::getDBO();
		$query = 'INSERT INTO #__aurorasim_user'
		. ' (uuid, joomla_userid)'
		. ' values ('. $db->quote( $uuid ) .', '. $joomlaid .') ';
		$db->setQuery( $query );
		return $db->query();
	}
	
	function GetUserUUID($user)
	{
		$db =& JFactory::getDBO();
		$query = 'SELECT `uuid`'
			. ' FROM #__aurorasim_user'
			. ' WHERE joomla_userid=' . $user["id"];
		$db->setQuery( $query );
		$result = $db->loadResult();
		return $result;
	}
	
	function CreateAuroraUser($user)
	{
		if (!$this->CheckAuroraUserExists($user))
		{
			$aconfig = $this->GetConfigSettings();
			$userIP = "";
			if ($_SERVER["HTTP_X_FORWARDED_FOR"]) {
				$userIP = $_SERVER["HTTP_X_FORWARDED_FOR"];
			} elseif ($_SERVER["REMOTE_ADDR"]) {
				$userIP = $_SERVER["REMOTE_ADDR"];
			} else {
				$userIP = "This user has no ip";
			}
			
			$usersParams = &JComponentHelper::getParams( 'com_users' );
			$useractivation = $usersParams->get( 'useractivation' ); // in this example, we load the config-setting
			if ($useractivation == 1)
				$useractivation = -1;
			else
				$useractivation = 0;
				
			$nameexplosion = explode ( ' ' , $user["name"] );
			$found = array();
			$found[0] = json_encode(array('Method' => 'CreateAccount', 'WebPassword' => md5($aconfig['webui_password']),
						'Name' => $user["username"],
						'Email' => $user["email"],
						'HomeRegion' => $aconfig['default_home'],
						'PasswordHash' => $_POST['password'],
						'PasswordSalt' => "",
						'AvatarArchive' => $_POST['AvatarArchive'],
						'UserLevel' => $useractivation,
						'RLFisrtName' => $nameexplosion[0],
						'RLLastName' => $nameexplosion[1],
						'RLAdress' => "",
						'RLCity' => "",
						'RLZip' => "",
						'RLCountry' => "",
						'RLDOB' => "31/12/1900",
						'RLIP' => $userIP
						));
						
						
			$do_post_requested = $this->do_post_request($found);
			$recieved = json_decode($do_post_requested);

			
			// echo '<pre>';
			// var_dump($recieved);
			// var_dump($do_post_requested);
			// echo '</pre>';

			if ($recieved->{'Verified'} == 1) 
			{
				return $this->CreateAuroraJoomlaLink($recieved->{'UUID'}, $user["id"]);
			}
		}
		return false;
	}
	
	function GetUserProfile($user)
	{
		$aconfig = $this->GetConfigSettings();
		$found = array();
		$found[0] = json_encode(array('Method' => 'GetProfile', 'WebPassword' => md5($aconfig['webui_password']), 'Name' => $user["username"], 'UUID' => ''));
		$do_post_requested = $this->do_post_request($found);
		$recieved = json_decode($do_post_requested);

		// echo '<pre>';
		// var_dump($recieved);
		// var_dump($do_post_requested);
		// echo '</pre>';
		
		return $recieved;
	}
	
	function do_post_request($found) 
	{
		$aconfig = $this->GetConfigSettings();
		$params = array('http' => array(
				'method' => 'POST',
				'content' => implode(',', $found)
				));
		$ctx = stream_context_create($params);
		$timeout = 15;
		$old = ini_set('default_socket_timeout', $timeout);
		$fp = @fopen($aconfig['webui_url'], 'rb', false, $ctx);
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
}
?>
