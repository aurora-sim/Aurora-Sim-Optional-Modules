<?php defined('_JEXEC') or die('Restricted access'); ?>

<form action="index.php" method="post" name="adminForm" id="adminForm">
<div class="col100">
	<fieldset class="adminform">
		<legend><?php echo JText::_( 'Details' ); ?></legend>

		<table class="admintable">
		<tr>
			<td width="100" align="right" class="key">
				<label for="webui_gridname">
					<?php echo JText::_( 'Grid Name' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="webui_gridname" id="webui_gridname" size="32" maxlength="250" value="<?php echo $this->aurorasim->webui_gridname;?>" />
			</td>
		</tr>
		<tr>
			<td width="100" align="right" class="key">
				<label for="webui_url">
					<?php echo JText::_( 'WebUI URL' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="webui_url" id="webui_url" size="32" maxlength="250" value="<?php echo $this->aurorasim->webui_url;?>" />
			</td>
		</tr>
		<tr>
			<td width="100" align="right" class="key">
				<label for="webui_texture_url">
					<?php echo JText::_( 'WebUI Texture Server URL' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="webui_texture_url" id="webui_texture_url" size="32" maxlength="250" value="<?php echo $this->aurorasim->webui_texture_url;?>" />
			</td>
		</tr>
		<tr>
			<td width="100" align="right" class="key">
				<label for="webui_password">
					<?php echo JText::_( 'WebUI Password' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="webui_password" id="webui_password" size="32" maxlength="250" value="<?php echo $this->aurorasim->webui_password;?>" />
			</td>
		</tr>
		<tr>
			<td width="100" align="right" class="key">
				<label for="default_home">
					<?php echo JText::_( 'Default Home' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="default_home" id="default_home" size="32" maxlength="250" value="<?php echo $this->aurorasim->default_home;?>" />
			</td>
		</tr>
		<tr>
			<td width="100" align="right" class="key">
				<label for="isdefault">
					<?php echo JText::_( 'Is Default (0 or 1)' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="isdefault" id="isdefault" size="32" maxlength="250" value="<?php echo $this->aurorasim->isdefault;?>" />
			</td>
		</tr>
		<tr>
			<td width="100" align="right" class="key">
				<label for="aurora_database_type">
					<?php echo JText::_( 'Aurora Sim Database Type' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="aurora_database_type" id="aurora_database_type" size="32" maxlength="250" value="<?php echo $this->aurorasim->aurora_database_type;?>" />
			</td>
		</tr>
		<tr>
			<td width="100" align="right" class="key">
				<label for="aurora_database_host">
					<?php echo JText::_( 'Aurora Sim Database Host' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="aurora_database_host" id="aurora_database_host" size="32" maxlength="250" value="<?php echo $this->aurorasim->aurora_database_host;?>" />
			</td>
		</tr>
		<tr>
			<td width="100" align="right" class="key">
				<label for="aurora_database_name">
					<?php echo JText::_( 'Aurora Sim Database Name' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="aurora_database_name" id="aurora_database_name" size="32" maxlength="250" value="<?php echo $this->aurorasim->aurora_database_name;?>" />
			</td>
		</tr>
		<tr>
			<td width="100" align="right" class="key">
				<label for="aurora_database_user">
					<?php echo JText::_( 'Aurora Sim Database User' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="aurora_database_user" id="aurora_database_user" size="32" maxlength="250" value="<?php echo $this->aurorasim->aurora_database_user;?>" />
			</td>
		</tr>
		<tr>
			<td width="100" align="right" class="key">
				<label for="aurora_database_pass">
					<?php echo JText::_( 'Aurora Sim Database Pass' ); ?>:
				</label>
			</td>
			<td>
				<input class="text_area" type="text" name="aurora_database_pass" id="aurora_database_pass" size="32" maxlength="250" value="<?php echo $this->aurorasim->aurora_database_pass;?>" />
			</td>
		</tr>
	</table>
	</fieldset>
</div>
<div class="clr"></div>

<input type="hidden" name="option" value="com_aurorasim" />
<input type="hidden" name="id" value="<?php echo $this->aurorasim->id; ?>" />
<input type="hidden" name="task" value="" />
<input type="hidden" name="controller" value="aurorasim" />
</form>
