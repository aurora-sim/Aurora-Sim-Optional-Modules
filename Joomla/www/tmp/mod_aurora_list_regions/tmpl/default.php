<?php defined('_JEXEC') or die('Restricted access'); // no direct access ?>
<?
function curPageURL2() {
	$pageURL = 'http';
	if ($_SERVER["HTTPS"] == "on") {$pageURL .= "s";}
	$pageURL .= "://";
	if ($_SERVER["SERVER_PORT"] != "80") {
		$pageURL .= $_SERVER["SERVER_NAME"].":".$_SERVER["SERVER_PORT"].$_SERVER["REQUEST_URI"];
	} else {
		$pageURL .= $_SERVER["SERVER_NAME"].$_SERVER["REQUEST_URI"];
	}
	if ($_GET['option'] != '')
	{
		$pageURL = strtok($pageURL,'?');
		$pageURL = $pageURL . "?option=" . $_GET['option'] . "&view=" . $_GET["view"] . "&id=" . $_GET['id'] . "&Itemid=" . $_GET['Itemid'];
	}
	return $pageURL;
}

//get live_site
if(defined('_JEXEC')){
   //joomla 1.5               
   $live_site = JURI::root();               
}else{
   //joomla 1.0.x
   $live_site = $mosConfig_live_site;
}
?>
<div id="regionlist">
	<div id="info"><p> <?php echo JText::_('REGION_LIST'); ?></p></div>
	<table width="100%">
		<tr>
			<td>
				<p><?=$count; ?> <?php echo JText::_('REGIONS_FOUND'); ?><p>
			</td>
			<td>
			<div id="region_navigation">
				<table>
					<tr>
						<td>
							<a href="<?=curPageURL2()?>&AStart=0&amp;ALimit=<?=$ALimit?>" target="_self" title="<?php echo JText::_('PAGINATION_TOOLTIPS_BACK_BEGIN'); ?>">
								<img SRC="<?=$live_site?>modules/mod_aurora_list_regions/icons/icon_back_more_<? if(0 > ($AStart - $ALimit)) echo off; else echo on ?>.gif" WIDTH=15 HEIGHT=15 border="0" />
							</a>
						</td>
						<td>
							<a href="<?=curPageURL2()?>&AStart=<? if(0 > ($AStart - $ALimit)) echo 0; else echo $AStart - $ALimit; ?>&amp;ALimit=<?=$ALimit?>" target="_self"  title="<?php echo JText::_('PAGINATION_TOOLTIPS_BACK_PAGE'); ?>">
								<img SRC="<?=$live_site?>modules/mod_aurora_list_regions/icons/icon_back_one_<? if(0 > ($AStart - $ALimit)) echo off; else echo on ?>.gif" WIDTH=15 HEIGHT=15 border="0" />
							</a>
						</td>
						<td>
						  	<p><?php echo JText::_('NAVIGATION_PAGE'); ?> <?=$sitestart ?> <?php echo JText::_('NAVIGATION_OF'); ?> <?=$sitemax ?></p>
						</td>
						<td>
							<a href="<?=curPageURL2()?>&AStart=<? if($count <= ($AStart + $ALimit)) echo 0; else echo $AStart + $ALimit; ?>&amp;ALimit=<?=$ALimit?>" target="_self" title="<?php echo JText::_('PAGINATION_TOOLTIPS_FORWARD_PAGE'); ?>">
								<img SRC="<?=$live_site?>modules/mod_aurora_list_regions/icons/icon_forward_one_<? if($count <= ($AStart + $ALimit)) echo off; else echo on ?>.gif" WIDTH=15 HEIGHT=15 border="0" />
							</a>
						</td>
						<td>
							<a href="<?=curPageURL2()?>&AStart=<? if(0 > ($count <= ($AStart + $ALimit))) echo 0; else echo ($sitemax - 1) * $ALimit; ?>&amp;ALimit=<?=$ALimit?>" target="_self"  title="<?php echo JText::_('PAGINATION_TOOLTIPS_LAST_PAGE'); ?>">
								<img SRC="<?=$live_site?>modules/mod_aurora_list_regions/icons/icon_forward_more_<? if($count <= ($AStart + $ALimit)) echo "off"; else echo "on" ?>.gif" WIDTH=15 HEIGHT=15 border="0" />
							</a>
						</td>
						<td></td>
						<td>
							<a href="<?=curPageURL2()?>&AStart=0&amp;ALimit=10&amp;" target="_self" title="<?php echo JText::_('PAGINATION_TOOLTIPS_SHOW10'); ?>">
								<img SRC="<?=$live_site?>modules/mod_aurora_list_regions/icons/<? if($ALimit != 10) echo icon_limit_10_on; else echo icon_limit_off; ?>.gif" WIDTH=15 HEIGHT=15 border="0" ALT="<?php echo JText::_('PAGINATION_TOOLTIPS_LIMIT10'); ?>" />
							</a>
						</td>
						<td>
							<a href="<?=curPageURL2()?>&AStart=0&amp;ALimit=25&amp;" target="_self" title="<?php echo JText::_('PAGINATION_TOOLTIPS_SHOW25'); ?>">
								<img SRC="<?=$live_site?>modules/mod_aurora_list_regions/icons/<? if($ALimit != 25) echo icon_limit_25_on; else echo icon_limit_off; ?>.gif" WIDTH=15 HEIGHT=15 border="0" ALT="<?php echo JText::_('PAGINATION_TOOLTIPS_LIMIT25'); ?>" />
							</a>
						</td>
						<td>
							<a href="<?=curPageURL2()?>&AStart=0&amp;ALimit=50&amp;" target="_self" title="<?php echo JText::_('PAGINATION_TOOLTIPS_SHOW50'); ?>">
								<img SRC="<?=$live_site?>modules/mod_aurora_list_regions/icons/<? if($ALimit != 50) echo icon_limit_50_on; else echo icon_limit_off; ?>.gif" WIDTH=15 HEIGHT=15 border="0" ALT="<?php echo JText::_('PAGINATION_TOOLTIPS_LIMIT50'); ?>" />
							</a>
						</td>
						<td>
							<a href="<?=curPageURL2()?>&AStart=0&amp;ALimit=100&amp;" target="_self" title="<?php echo JText::_('PAGINATION_TOOLTIPS_SHOW100'); ?>">
								<img SRC="<?=$live_site?>modules/mod_aurora_list_regions/icons/<? if($ALimit != 100) echo icon_limit_100_on; else echo icon_limit_off; ?>.gif" WIDTH=15 HEIGHT=15 border="0" ALT="<?php echo JText::_('PAGINATION_TOOLTIPS_LIMIT100'); ?>" />
							</a>
						</td>
					</tr>
				</table>
				</div>
			</td>
		</tr>
	</table>
	<table width="100%">
		<thead>
			<tr>
				<td width="40%">
					<a href="<?=curPageURL2()?>&order=name" title="<?php echo JText::_('PAGINATION_TOOLTIPS_SORTN'); ?>"><p><?php echo JText::_('REGION_NAME'); ?></p></a>
				</td>
				<td width="20%">
					<a href="<?=curPageURL2()?>&order=owner" title="<?php echo JText::_('PAGINATION_TOOLTIPS_SORTX'); ?>"><p><?php echo JText::_('OWNER'); ?></p></a>
				</td>
				<td width="20%">
					<a href="<?=curPageURL2()?>&order=x" title="<?php echo JText::_('PAGINATION_TOOLTIPS_SORTX'); ?>"><p><?php echo JText::_('LOCATION'); ?>: X</p></a>
				</td>
				<td width="20%">
					<a href="<?=curPageURL2()?>&order=y" title="<?php echo JText::_('PAGINATION_TOOLTIPS_SORTY'); ?>"><p><?php echo JText::_('LOCATION'); ?>: Y</p></a>
				</td>
				
			</tr>
		</thead>
		<tbody>
			<tr>
				<td colspan="5">
					<script language="javascript">
						// Based on: http://www.quirksmode.org/js/findpos.html
						var getCumulativeOffset = function (obj) {
							var left, top;
							left = top = 0;
							if (obj.offsetParent) {
								do {
									left += obj.offsetLeft;
									top  += obj.offsetTop;
								} while (obj = obj.offsetParent);
							}
							return {
								x : left,
								y : top
							};
						};
						var ignoreNext = 0;
						var showMenu = function(el, menu) 
						{
							if (ignoreNext == 0)
							{
								menu.style.position = 'absolute';
								menu.style.zIndex = 5000;
								menu.style.display = 'block';
							}
							ignoreNext = 0;
						};
					</script>
					<table width="100%">
						<tbody>
						<?
							$w=0;
							foreach ($items as $item) 
							{
								$recieved = json_decode($item['Info']);
								$serverIP = $recieved->{'serverIP'};
								$serverHttpPort = $recieved->{'serverHttpPort'};

								$SERVER = "http://".$serverIP.":".$serverHttpPort;
								$UUID = str_replace("-", "", $item['RegionUUID']);
								$source = $SERVER . "/index.php?method=regionImage" . $UUID;
								$w++;
						?>
							
							<tr class="<? echo ($odd = $w%2 )? "even":"odd" ?>" onmouseover="showMenu(this, document.getElementById('regionfloater_<?=$UUID;?>'))" onmouseout="document.getElementById('regionfloater_<?=$UUID;?>').style.display='none';" >
								<td height="64" valign="top" width="40%" style="background-repeat:no-repeat; background-position:left top; background-image:url('<?=$source;?>'); color:white; font-size:16px;  ">
									<div style="border-style:solid;border-width:1px; position: absolute; display: none; width:256px;height:256px;background-image:url('<?=$source;?>');" id="regionfloater_<?=$UUID;?>">
										<table width="256" height="256" cellpadding="0" cellspacing="0">
											<tr>
												<td valign="top" height="64"><?=$item['RegionName']?></td>
											</tr>
											<tr onmouseover="ignoreNext = 1; document.getElementById('regionfloater_<?=$UUID;?>').style.display='none';">
												<td height="192">
													<div >
													<img src="<?=$live_site?>images/blank.png" width="256" height="192" />
													</div>
												</td>
											</tr>
										</table>
										
									</div>
									<div  id="placeholder_<?=$UUID;?>" ><?=$item['RegionName']?></div>
								</td>
								<td width="20%" valign="top">
									<?
										if (($item['FirstName'] == "wendell") && ($item['LastName'] == "Thor"))
										{
											echo "Gay Nations";
										}
										else
										{
											echo $item['FirstName']." ".$item['LastName'];
										}
									?>
								</td>
								<td width="20%" valign="top"><?=$item['LocX']/256;?></td>
								<td width="20%" valign="top"><?=$item['LocY']/256;?></td>
							</tr>
						<?}?>
						</tbody>
						<tr>
							<td colspan="4"><img src="<?=$live_site?>images/blank.png" height="192"  />
						</tr>
					</table>
				</td>
			</tr>
		</tbody>
	</table>
	
</div>