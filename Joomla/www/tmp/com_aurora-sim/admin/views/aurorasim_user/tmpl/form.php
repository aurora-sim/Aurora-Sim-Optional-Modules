<?php defined('_JEXEC') or die('Restricted access'); ?>

<form action="index.php" method="post" name="adminForm" id="adminForm">
<div class="col100">
	<fieldset class="adminform">
		<legend><?php echo JText::_( 'Details' ); ?></legend>

		<table class="admintable">
		<tr>
			<td width="100" align="right" class="key">
				<label for="uuid">
					<?php echo JText::_( 'UUID' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="uuid" id="uuid" size="32" maxlength="250" value="<?php echo $this->aurorasim_user->uuid;?>" />
			</td>
			<td>
				<label for="joomla_userid">
					<?php echo JText::_( 'Joomla User ID' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="joomla_userid" id="joomla_userid" size="32" maxlength="250" value="<?php echo $this->aurorasim_user->joomla_userid;?>" />
			</td>
		</tr>
	</table>
	</fieldset>
</div>
<div class="clr"></div>

<input type="hidden" name="option" value="com_aurorasim" />
<input type="hidden" name="id" value="<?php echo $this->aurorasim_user->id; ?>" />
<input type="hidden" name="task" value="" />
<input type="hidden" name="controller" value="aurorasim_user" />
</form>
