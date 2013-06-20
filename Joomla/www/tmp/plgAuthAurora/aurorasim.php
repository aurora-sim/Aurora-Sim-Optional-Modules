<?php
/**
 * @version    $Id: myauth.php 7180 2007-04-23 16:51:53Z jinx $
 * @package    Joomla.Tutorials
 * @subpackage Plugins
 * @license    GNU/GPL
 */
 
// Check to ensure this file is included in Joomla!
defined('_JEXEC') or die();
 
jimport('joomla.event.plugin');
jimport('joomla.application.component.helper'); // include libraries/application/component/helper.php

 
/**
 * Example Authentication Plugin.  Based on the example.php plugin in the Joomla! Core installation
 *
 * @package    Joomla.Tutorials
 * @subpackage Plugins
 * @license    GNU/GPL
 */
class plgAuthenticationAuroraSim extends JPlugin
{
	var $configSettings = Array();
	
   /**
     * Constructor
     *
     * For php4 compatability we must not use the __constructor as a constructor for plugins
     * because func_get_args ( void ) returns a copy of all passed arguments NOT references.
     * This causes problems with cross-referencing necessary for the observer design pattern.
     *
     * @param object $subject The object to observe
     * @since 1.5
     */
    function plgAuthenticationAuroraSim(& $subject) {
        parent::__construct($subject);
		$this->_plugin = JPluginHelper::getPlugin( 'User', 'Adduser' );
		$this->_params = new JParameter( $this->_plugin->params );
    }
	
	/**
	 * Example store user method
	 *
	 * Method is called after user data is stored in the database
	 *
	 * @param 	array		holds the new user data
	 * @param 	boolean		true if a new user is stored
	 * @param	boolean		true if user was succesfully stored in the database
	 * @param	string		message
	 */
	function onAfterStoreUser( $user, $isnew, $success, $msg )
	{
		// echo '<pre>';
		// var_dump($user);
		// var_dump($msg);
		// echo '</pre>';
		
		// throw new Exception("Problem with nothing.. just testing");
		
		$usersParams = &JComponentHelper::getParams( 'com_users' );
		$useractivation = $usersParams->get( 'useractivation' ); // in this example, we load the config-setting
		if (($isnew) && ($success))
		{
			$this->CreateAuroraUser($user);
		}
	}
	
	
 
    /**
     * This method should handle any authentication and report back to the subject
     * This example uses simple authentication - it checks if the password is the reverse
     * of the username (and the user exists in the database).
     *
     * @access    public
     * @param     array     $credentials    Array holding the user credentials ('username' and 'password')
     * @param     array     $options        Array of extra options
     * @param     object    $response       Authentication response object
     * @return    boolean
     * @since 1.5
     */
    function onAuthenticate( $credentials, $options, &$response )
    {
        /*
         * Here you would do whatever you need for an authentication routine with the credentials
         *
         * In this example the mixed variable $return would be set to false
         * if the authentication routine fails or an integer userid of the authenticated
         * user if the routine passes
         */
		$USERNAME = $credentials['username'];
		$PASSWORD = $credentials['password'];
		

		
		$auroraResponse = $this->login($USERNAME, $PASSWORD);
		$arrayCount = count($auroraResponse);

		if ($arrayCount == 0) 
		{
			$response->status = JAUTHENTICATE_STATUS_FAILURE;
			$response->error_message = 'Invalid username password combo';
		}
		else
		{
		
			$db =& JFactory::getDBO();
			$query = 'SELECT `joomla_userid`'
				. ' FROM #__aurorasim_user'
				. ' WHERE uuid=' . $db->quote( $auroraResponse['USERID'] );
			$db->setQuery( $query );
			$result = $db->loadResult();
		 
			if (!$result) 
			{
				$newuser = $this->createUser($auroraResponse['FIRSTNAME'].' '.$auroraResponse['LASTNAME'], $auroraResponse['FIRSTNAME'], $auroraResponse['LASTNAME'], $auroraResponse['EMAIL'], $PASSWORD);
				
				if (!$newuser)
				{
					$response->status = JAUTHENTICATE_STATUS_FAILURE;
					$response->error_message = 'Error Importing user';
				}
				else
				{
					$linkResults = $this->CreateAuroraJoomlaLink($auroraResponse['USERID'], $newuser->id);
					if (!$linkResults)
					{
						$response->status = JAUTHENTICATE_STATUS_FAILURE;
						$response->error_message = 'Error insertin refernce';
					}
					else
					{
						$response->email = $newuser->email;
						$response->status = JAUTHENTICATE_STATUS_SUCCESS;
					}
				}
			}
			else
			{
				$email = JUser::getInstance($result); // Bring this in line with the rest of the system
				$response->email = $email->email;
				$response->status = JAUTHENTICATE_STATUS_SUCCESS;
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
		
		// echo '<pre>';
		// var_dump($this->configSettings);
		// var_dump($results);
		// echo '</pre>';
		// throw new Exception("Problem with nothing.. just testing");
		
		
		
		return $this->configSettings;
	}
	
	function CheckAuroraUserExists($user)
	{
		$aconfig = $this->GetConfigSettings();
		$thisusername = $user["username"];
		$found = array();
		
		$found[0] = json_encode(array('Method' => 'CheckIfUserExists', 'WebPassword' => md5($aconfig['webui_password']), 'Name' => $thisusername));
		
		$do_post_requested = $this->do_post_request($found);
		$recieved = json_decode($do_post_requested);

		$returnValue = $recieved->{'Verified'} == 1;	
		
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

			$nameexplosion = explode ( ' ' , $user["name"] );
			$found = array();
			$found[0] = json_encode(array('Method' => 'CreateAccount', 'WebPassword' => md5($aconfig['webui_password']),
						'Name' => $user["username"],
						'Email' => $user["email"],
						'HomeRegion' => "f0aef4e8-c839-4eba-af7c-fa77b1490e03",
						'PasswordHash' => $_POST['password'],
						'PasswordSalt' => "",
						'AvatarArchive' => "",
						'UserLevel' => 0,
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
	
	function createUser($username, $firstname, $lastname, $email, $password)
	{
		/*
		 
		I handle this code as if it is a snippet of a method or function!!
		 
		First set up some variables/objects
		*/
		// get the ACL
		$acl =& JFactory::getACL();
		 
		/* get the com_user params */
		 
		jimport('joomla.application.component.helper'); // include libraries/application/component/helper.php
		$usersParams = &JComponentHelper::getParams( 'com_users' ); // load the Params
		 
		// "generate" a new JUser Object
		$user = JFactory::getUser(0); // it's important to set the "0" otherwise your admin user information will be loaded
		 
		$data = array(); // array for all user settings
		 
		// get the default usertype
		$usertype = $usersParams->get( 'new_usertype' );
		if (!$usertype) {
			$usertype = 'Registered';
		}
		 
		// set up the "main" user information
		 
		$data['name'] = $firstname.' '.$lastname; // add first- and lastname
		$data['username'] = $username; // add username
		$data['email'] = $email; // add email
		$data['gid'] = $acl->get_group_id( '', $usertype, 'ARO' );  // generate the gid from the usertype
		 
		/* no need to add the usertype, it will be generated automaticaly from the gid */
		 
		$data['password'] = $password; // set the password
		$data['password2'] = $password; // confirm the password
		$data['sendEmail'] = 1; // should the user receive system mails?
		 
		/* Now we can decide, if the user will need an activation */
		 
		$useractivation = $usersParams->get( 'useractivation' ); // in this example, we load the config-setting
		$data['block'] = 0; // don't block the user
		 
		if (!$user->bind($data)) { // now bind the data to the JUser Object, if it not works....
		 
			JError::raiseWarning('', JText::_( $user->getError())); // ...raise an Warning
			return false; // if you're in a method/function return false
		}
		 
		if (!$user->save()) { // if the user is NOT saved...
		 
			JError::raiseWarning('', JText::_( $user->getError())); // ...raise an Warning
			return false; // if you're in a method/function return false
		 
		}
		 
		return $user; // else return the new JUser object
	}
	
	function login($username, $password)
	{
		$aconfig = $this->GetConfigSettings();
		
		$returnValue = array();
		$found = array();

		$found[0] = json_encode(array('Method' => 'Login', 'WebPassword' => md5($aconfig['webui_password']),
									 'Name' => $username,
									 'Password' => $password));

		$do_post_request = $this->do_post_request($found);
		$recieved = json_decode($do_post_request);

		$UUIDC = $recieved->{'UUID'};

		if ($recieved->{'Verified'} == 1) 
		{	
			$returnValue['USERID'] = $UUIDC;
			$returnValue['NAME'] = $recieved->{'FirstName'} . ' ' . $recieved->{'LastName'};
			$returnValue['FIRSTNAME'] = $recieved->{'FirstName'};
			$returnValue['LASTNAME'] = $recieved->{'LastName'};
			$returnValue['EMAIL'] = $recieved->{'Email'};

			$found[0] = json_encode(array('Method' => 'SetWebLoginKey', 'WebPassword' => md5($aconfig['webui_password']),
									 'PrincipalID' => $UUIDC));
			$do_post_request = $this->do_post_request($found);
			$recieved = json_decode($do_post_request);
			$WEBLOGINKEY = $recieved->{'WebLoginKey'};
			$returnValue['WEBLOGINKEY'] = $WEBLOGINKEY;
		}
		return $returnValue;
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