<?php defined('_JEXEC') or die('Restricted access'); // no direct access ?>
<?
	if (!$user->guest) 
	{
	
		function RecurseXML($xml,&$vals,$parent="") 
		{ 
			$childs=0; 
			$child_count=-1; # Not realy needed. 
			$arr=array(); 
			$lastKey = "";
			
			foreach ($xml->children() as $key=>$value) { 
				if (in_array($key,$arr)){ 
					$child_count++; 
				} else { 
					$child_count=0; 
				} 
				$arr[]=$key; 
				$k=($parent == "") ? "$key.$child_count" : "$parent.$key.$child_count"; 
				
				$childs=RecurseXML($value,$vals,$k); 
				if ($childs==0) 
				{
					if ($lastKey == "Image")
						$vals["Image"]= (string)$value; 
				} 
				$lastKey = (string)$value;
			} 
			return $childs; 
		} 
		
		echo "<div id=\"friendlist\" style=\"height:300px;overflow:scroll;overflow-x:hidden;overflow-y:auto; \"><table>";
		$section = "-1";
		foreach ($items as $item) 
		{
			
			try
			{
				$temp_value = preg_replace('/[^(\x20-\x7F)]*/','', $item["Value"]);
				$xmlResult = new SimpleXMLElement($temp_value);
				$vals = array(); 
				RecurseXML($xmlResult,$vals);
			}
			catch (Exception $e)
			{
				$vals = array(); 
				$vals["Image"] = "00000000-0000-0000-0000-000000000000";
			}
		
		?>
			<?if ($section != $item["IsOnline"]) { ?>
				<tr>
					<td style="font-weight:bold;">
						<?=($item["IsOnline"] == "1") ? "Online" : "Offline"; ?>
					</td>
				</tr>
			<? $section = $item["IsOnline"]; }?>
			<tr>
				<td> 
					<? if ($vals["Image"] != "00000000-0000-0000-0000-000000000000") { ?>
						<img src="<?=$textureServer?>/index.php?method=GridTexture&uuid=<?=$vals["Image"]?>" style="float:right;width:32px;height:32px; " />
					<?}else{?>
						<img src="images/blank.png" style="float:right;width:32px;height:32px;" />
					<?}?>
					<?=$item["Name"]?>
				</td>
			</tr>
		<?}
		echo "</table></div>";
	}else{
		echo JText::_('LOGIN_TO_SEE_FRIENDS');
	}
?>
	
