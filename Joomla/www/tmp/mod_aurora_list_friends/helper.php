<?php
defined('_JEXEC') or die('Direct Access to this location is not allowed.');
 
class ModAurora_List_FriendsHelper
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
	
    /**
     * Returns a list of post items
    */
    public function getItems($user)
    {
        $aconfig =  ModAurora_List_FriendsHelper::GetConfigSettings();
		$UUID = ModAurora_List_FriendsHelper::GetUserUUID($user);
			
		$ORDERBY = "ORDER BY ui.IsOnline DESC, Name ASC";
		//if($_GET['order']=="name"){
		//	$ORDERBY=" ORDER by g.regionName ASC";
		//}else if($_GET['order']=="staus"){
		//	$ORDERBY=" ORDER by g.locX ASC";
		//}
		
		//$AnzeigeStart = 0;
		//if($_GET[AStart]){$AStart=$_GET[AStart];};
		//if(!$AStart) $AStart = $AnzeigeStart;

		//$ALimit = 10;
		//if ($_GET['ALimit']) $ALimit = $_GET['ALimit'];
		//$Limit = "LIMIT ".$AStart.",".$ALimit;
		
		
		
		//$count = ModAurora_List_FriendsHelper::GetCount('FROM  friends WHERE PrincipalID = \''.$UUID.'\'', $aconfig);
		//if ($count)
		//{
			//$sitemax=ceil($count / $ALimit);
			//$sitestart=ceil($AStart / $ALimit)+1;
			//if($sitemax == 0){$sitemax=1;}
				
			$options = array();
			$option['driver'] 	= $aconfig['aurora_database_type'];
			$option['host']     = $aconfig['aurora_database_host'];
			$option['database'] = $aconfig['aurora_database_name'];
			$option['user']     = $aconfig['aurora_database_user'];
			$option['password'] = $aconfig['aurora_database_pass'];
			
			$query = "SELECT u.Name, ui.IsOnline, ud.Value FROM friends as f INNER JOIN useraccounts as u on u.PrincipalID = f.Friend INNER JOIN userinfo as ui on ui.UserID = f.Friend INNER JOIN userdata as ud on ud.ID = f.Friend AND ud.Key = 'LLProfile'  WHERE f.PrincipalID = '".$UUID."' ".$ORDERBY; //." ".$Limit;
			
			
			
			$db = &JDatabase::getInstance( $option );
			$db->setQuery( $query );
			$items = ($items = $db->loadAssocList())?$items:array();

			$returnValue = array();
			$returnValue[0] = $items;
			$returnValue[1] = $aconfig['webui_texture_url'];
			//$returnValue[0] = $count;
			//$returnValue[1] = $AStart;
			//$returnValue[2] = $ALimit;
			//$returnValue[3] = $sitemax;
			//$returnValue[4] = $sitestart;
			//$returnValue[5] = $items;
			
			// echo '<pre>';
			// var_dump($query);
			// echo '</pre>';
			
			return $returnValue;
		// }
    } //end getItems
	
	function GetCount($QueryPart, $aconfig)
	{
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
		return $aconfigSettings;
	}
 
} //end ModHelloWorld2Helper
?>