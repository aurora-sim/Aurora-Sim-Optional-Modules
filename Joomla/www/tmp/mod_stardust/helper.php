<?php
defined('_JEXEC') or die('Direct Access to this location is not allowed.');
 
class ModStarDustHelper
{

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

	function getItems($user)
	{
		$aconfig =  ModStarDustHelper::GetConfigSettings();
		$UUID = ModStarDustHelper::GetUserUUID($user);
		if ($UUID)
		{
			
			$ORDERBY = "";
			if($_GET['order']=="RegionName")
				$ORDERBY=" ORDER by RegionName ASC";
			else
				$ORDERBY=" ORDER by Created DESC";

			if($_GET['AStart']){$AStart=$_GET['AStart'];};
			if(!$AStart) $AStart = 0;

			$ALimit = 10;
			$Limit = "LIMIT ".$AStart.",".$ALimit;
			$NityDaysAgo = microtime(true) - 7776000;
			$QueryPart = " FROM stardust_currency_history WHERE (ToPrincipalID = '".$UUID."' OR FromPrincipalID = '".$UUID."') AND Created >= ".$NityDaysAgo." AND Complete = 1";

			$count = ModStarDustHelper::GetCount($QueryPart);
			// $count = 100;
			
			if ($count)
			{
				
				$sitemax=ceil($count / 10);
				$sitestart=ceil($AStart / 10)+1;
				if($sitemax == 0){$sitemax=1;}
				
				$options = array();
				$option['driver'] 	= $aconfig['aurora_database_type'];
				$option['host']     = $aconfig['aurora_database_host'];
				$option['database'] = $aconfig['aurora_database_name'];
				$option['user']     = $aconfig['aurora_database_user'];
				$option['password'] = $aconfig['aurora_database_pass'];
				
				$query = "SELECT TransactionID, Description, FromPrincipalID, FromName, FromObjectID, FromObjectName, ToPrincipalID, ToName, ToObjectID, ToObjectName, Amount, Complete, CompleteReason, RegionName, RegionID, RegionPos, TransType, Created, Updated, ToBalance, FromBalance $QueryPart $ORDERBY $Limit";

				$db = &JDatabase::getInstance( $option );

				$db->setQuery( $query );

				$items = ($items = $db->loadAssocList())?$items:array();

				$returnValue = array();
				$returnValue[0] = $count;
				$returnValue[1] = $AStart;
				$returnValue[2] = $ALimit;
				$returnValue[3] = $UUID;
				$returnValue[4] = $sitemax;
				$returnValue[5] = $sitestart;
				$returnValue[6] = $items;
				
				return $returnValue;
			}
		}
	}
	
	function GetCount($QueryPart)
	{
		$aconfig = ModStarDustHelper::GetConfigSettings();
		$options = array();
		$option['driver'] 	= $aconfig['aurora_database_type'];
		$option['host']     = $aconfig['aurora_database_host'];
		$option['database'] = $aconfig['aurora_database_name'];
		$option['user']     = $aconfig['aurora_database_user'];
		$option['password'] = $aconfig['aurora_database_pass'];
		$db = &JDatabase::getInstance( $option );

		$query = "SELECT COUNT(*) ".$QueryPart ;
		$db->setQuery( $query );
		return $db->loadResult();
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
	

	
	function getmicrotime($theTime, $e = 7) 
	{ 
		list($u, $s) = explode(' ',theTime); 
		return bcadd($u, $s, $e); 
	} 
} //end ModHelloWorld2Helper
?>