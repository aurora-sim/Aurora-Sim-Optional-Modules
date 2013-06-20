<?php defined('_JEXEC') or die('Restricted access'); // no direct access ?>
<?php
function curPageURL() {
	$pageURL = 'http';
	if ($_SERVER["HTTPS"] == "on") {$pageURL .= "s";}
	$pageURL .= "://";
	if ($_SERVER["SERVER_PORT"] != "80") {
		$pageURL .= $_SERVER["SERVER_NAME"].":".$_SERVER["SERVER_PORT"].$_SERVER["REQUEST_URI"];
	} else {
		$pageURL .= $_SERVER["SERVER_NAME"].$_SERVER["REQUEST_URI"];
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
<table class="moduletable">
	<tr>
		<td>
			<?php
			if (empty($submitted))
			{
				foreach ($items as $item) {?>
					<form action="<?=curPageURL()?>" method="post">
						<div class="module">
							<h3><?php echo JText::_('NAME'); ?>: <?=$item['name'] ?></h3> 
							<img align="center" src="<?=$live_site?>modules/mod_stardust_land/images/SimImageStub.jpg" />
							<p><?=$item['description']?></p>
							<input type="hidden" name="idx" value="<?=$item['id']?>" />
							<input type="hidden" name="button_id" value="<?=$item['button_id']?>" />
							<input type="Submit" name="submit" value="Get It for only <?=$item['price'] / 100.0 ?>" />
						</div>
					</form>
					<br/>
				<?}
			}else{?>
				<form action="<?=curPageURL()?>" name="thisform" id="thisform" method="post">
					<table width="100%">
						<tr>
							<td style="color:red;"><?=$error;?></td>
						</tr>
						<tr>
							<td valign="top"><?php echo JText::_('REGION NAME'); ?></td>
						</tr>
						<tr>
							<td><input name="name" type="text" maxlength="36" value="<?=$_POST[name]?>"  /></td>
						</tr>
						<tr>
							<td  valign="top">
								<?if ($_POST[error] != ''){?>
									<div><?=$_POST[error]?></div>
								<?php } ?>
							</td>
						</tr>						
						<tr>
							<td valign="top"><?php echo JText::_('ADDITIONAL NOTES'); ?></td>
						</tr>
						<tr>
							<td><textarea name="notes" cols=30 rows=5 maxlength="1024"><?=$_POST[notes]?></textarea></td>
						</tr>
						<tr>
							<td valign="bottom">
								<?php echo JText::_('TERMS OF SERVICE'); ?>
							</td>
						</tr>
						<tr>
							<td>
								<div style="width:100%;height:100px;overflow:auto;"><?=$tos?></div>
							</td>
						</tr>
						<tr>
							<td>
								<input type="checkbox" name="agree" id="agree" value="1" /><label for="agree"><?php echo JText::_('I AGREE'); ?></label>
							</td>
						</tr>
						<tr>
							<td>
								<input type="submit" name="submit2" value="<?php echo JText::_('GET IT'); ?>" />
								<input type="hidden" name="idx" value="<?=$_POST["idx"]?>" />
							</td>
						</tr>
					</table>
				</form>
			<?php } ?>
		</td>
	</tr>
</table>