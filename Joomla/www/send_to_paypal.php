<?
define( '_JEXEC', 1 );
define( 'JPATH_BASE', realpath(dirname(__FILE__) ));
define( 'DS', DIRECTORY_SEPARATOR );
 
require_once ( JPATH_BASE .DS.'includes'.DS.'defines.php' );
require_once ( JPATH_BASE .DS.'includes'.DS.'framework.php' );
$mainframe =& JFactory::getApplication('site');
$mainframe->initialise();

$session =& JFactory::getSession();

?>
<html>
	<head>
		<title>Transfering you to PayPal, Please stand by..</title>
	</head>
	<body onload="document.getElementById('thisform').submit();">
		<table width="100%" height="100%">
			<tr><td align="center" valign="middle"><img src="modules/mod_stardust_purchase/images/loading.gif" /></td></tr>
		</table>
		<form action="https://<?=$session->get('PAYPAL_URL')?>/cgi-bin/webscr" method="post" name="ppform" id="thisform">
			<input type="hidden" name="cmd" value="<?=$session->get('purchase_type')?>" />
			
<?	if ($session->get('purchase_type') == '_xclick-subscriptions'){?>
			<input type="hidden" name="a3" value="<?=$session->get('paypalAmount')?>" />
			<input type="hidden" name="p3" value="1" />
			<input type="hidden" name="t3" value="M" />
			<input type="hidden" name="src" value="1" />
			<input type="hidden" name="sra" value="1" />
			<input type="hidden" name="modify" value="0" />
	<?}else{?>
			<input type="hidden" name="amount" value="<?=$session->get('paypalAmount')?>" />
			<input type="hidden" name="quantity" value="1" />
	<?}?>
			
			<input type="hidden" name="notify_url" value="<?=$session->get('NOTIFY_URL')?>" />
			
			<input type="hidden" name="upload" value="1" />
			<input type="hidden" name="business" value="<?=$session->get('PAYPAL_ACCOUNT')?>" />
			<input type="hidden" name="currency_code" value="USD" />
			
			<input type="hidden" name="item_name" value="<?=$session->get('paypalPurchaseItem')?>" />
			<input type="hidden" name="no_shipping" value="1" />
			
			<input type="hidden" name="custom" value="<?=$session->get('purchase_id')?>" />
			<INPUT TYPE="hidden" NAME="return" value="<?=$session->get('RETURN_URL')?>">
			<input type="image" name="submit" src="https://<?=$session->get('PAYPAL_URL')?>/en_US/i/logo/PayPal_mark_37x23.gif" align="left" style="margin-right:7px;">
		</form>
</body>
</html>


