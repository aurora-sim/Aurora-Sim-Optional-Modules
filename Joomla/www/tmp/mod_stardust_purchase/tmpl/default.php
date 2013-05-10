<?php defined('_JEXEC') or die('Restricted access'); // no direct access ?>
<?php 
$live_site = JURI::root();  
?>
<? if (($waserror == "2") || ($waserror == 2)) { ?>
	<table width="100%">
		<tr>
			<td><?php echo JText::_('PURCHASE_COMPLETE'); ?></td>
		</tr>
	</table>
<? } else if ($waserror == "1") { ?>
	<table width="100%">
		<tr>
			<td><?php echo JText::_('PURCHASE ISSUE'); ?></td>
		</tr>
	</table>
<?} else if ($waserror == "0") { 
	if ($_SESSION['purchase_id'] != '') {
?>
	<table width="100%">
		<tr>
			<td>
				<font><b><?php echo JText::_('PURCHASE WITH PAYPAL'); ?></b></font>
			</td>
			<td>
				<table>
					<tr>
						<td colspan="2"><?php echo JText::_('PLEASE REVIEW'); ?></td>
					</tr>
					<tr class="odd">
						<td><?php echo JText::_('YOU ARE BUYING'); ?></td>
						<td><?=$paypalPurchaseItem?></td>
					</tr>
					<tr class="even">
						<td><?php echo JText::_('YOU ARE PAYING'); ?></td>
						<td>$<?=$paypalAmount?> <?php echo JText::_('PAYMENT TYPE'); ?></td>
					</tr>
					<tr class="odd">
						<?if ($recieved->{'PurchaseType'} == 1){?>
							<td colspan="2">* <?php echo JText::_('ADDITIONAL FEE'); ?> <?=$AmountAdditionPerfectage?><?php echo JText::_('ADDITIONAL FEE2'); ?></td>
						<?}else{?>
							<td colspan="2">* <?php echo JText::_('MONTHLY FEE'); ?></td>
						<?}?>
					</tr>
					<tr>
						<td colspan="2">
							<a href="send_to_paypal.php"><img align="right" style="float:right" src="modules/mod_stardust_purchase/images/paypal-purchase-button.png" /></a>
						</td>
					</tr>
					
					<tr>
						<td colspan="2"><h2><?php echo JText::_('FAQ'); ?></h2></td>
					</tr>
					<tr class="odd">
						<td colspan="2"><?php echo JText::_('FAQ-Q1'); ?></td>
					</tr>
					<tr class="even">
						<td colspan="2"><?php echo JText::_('FAQ-A1'); ?></td>
					</tr>
					<tr class="odd">
						<td colspan="2"><?php echo JText::_('FAQ-Q2'); ?> <?=($AmountAdditionPerfectage * 100)?>?</td>
					</tr>
					<tr class="even">
						<td colspan="2"><?php echo JText::_('FAQ-A2'); ?></td>
					</tr>
					
				</table>				
			</td>
		</tr>
	</table>
<? } }?>