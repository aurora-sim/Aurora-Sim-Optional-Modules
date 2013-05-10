<?php
defined('_JEXEC') or die('Direct Access to this location is not allowed.');
 
class ModAurora_List_RegionsHelper
{
    /**
     * Returns a list of post items
    */
    public function getItems()
    {
        $aconfig =  ModAurora_List_RegionsHelper::GetConfigSettings();

			
		$ORDERBY = "";
		if($_GET['order']=="name"){
			$ORDERBY=" ORDER by g.regionName ASC";
		}else if($_GET['order']=="x"){
			$ORDERBY=" ORDER by g.locX ASC";
		}else if($_GET['order']=="y"){
			$ORDERBY=" ORDER by g.locY ASC";
		}else if($_GET['order']=="owner"){
			$ORDERBY=" ORDER by u.FirstName ASC";
		}else{
			$ORDERBY=" ORDER by g.RegionName ASC";
		}
		$AnzeigeStart = 0;
		if($_GET[AStart]){$AStart=$_GET[AStart];};
		if(!$AStart) $AStart = $AnzeigeStart;

		$ALimit = 10;
		if ($_GET['ALimit']) $ALimit = $_GET['ALimit'];
		$Limit = "LIMIT ".$AStart.",".$ALimit;
		
		$count = ModAurora_List_RegionsHelper::GetCount('FROM gridregions');
		if ($count)
		{
			$sitemax=ceil($count / $ALimit);
			$sitestart=ceil($AStart / $ALimit)+1;
			if($sitemax == 0){$sitemax=1;}
				
			$options = array();
			$option['driver'] 	= $aconfig['aurora_database_type'];
			$option['host']     = $aconfig['aurora_database_host'];
			$option['database'] = $aconfig['aurora_database_name'];
			$option['user']     = $aconfig['aurora_database_user'];
			$option['password'] = $aconfig['aurora_database_pass'];
			
			$query = "SELECT g.RegionName,g.LocX,g.LocY,g.OwnerUUID,g.Info,g.RegionUUID,u.FirstName,u.LastName FROM gridregions as g LEFT JOIN useraccounts as u on u.PrincipalID = g.OwnerUUID ".$ORDERBY." ".$Limit;
			$db = &JDatabase::getInstance( $option );
			$db->setQuery( $query );
			$items = ($items = $db->loadAssocList())?$items:array();

			$returnValue = array();
			$returnValue[0] = $count;
			$returnValue[1] = $AStart;
			$returnValue[2] = $ALimit;
			$returnValue[3] = $sitemax;
			$returnValue[4] = $sitestart;
			$returnValue[5] = $items;
			
			// echo '<pre>';
			// var_dump($returnValue);
			// echo '</pre>';
			
			return $returnValue;
		}
    } //end getItems
	
	function GetCount($QueryPart)
	{
		$aconfig = ModAurora_List_RegionsHelper::GetConfigSettings();
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
	
	function GetRegionDetails($x, $y)
	{
		$aconfig = ModAurora_List_RegionsHelper::GetConfigSettings();
		$options = array();
		$option['driver'] 	= $aconfig['aurora_database_type'];
		$option['host']     = $aconfig['aurora_database_host'];
		$option['database'] = $aconfig['aurora_database_name'];
		$option['user']     = $aconfig['aurora_database_user'];
		$option['password'] = $aconfig['aurora_database_pass'];
		$db = &JDatabase::getInstance( $option );

		$query = "SELECT RegionName,LocX,LocY,OwnerUUID,Info FROM gridregions WHERE LocX='" . $x . "' and LocY='" . $y . "'";
		$db->setQuery( $query );
		$items = ($items = $db->loadAssocList())?$items:array();
		return $items;
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