<?php
/**
 * AuroraSim View for AuroraSim World Component
 * 
 * @package    Joomla.Tutorials
 * @subpackage Components
 * @link http://docs.joomla.org/Developing_a_Model-View-Controller_Component_-_Part_4
 * @license		GNU/GPL
 */

// No direct access
defined( '_JEXEC' ) or die( 'Restricted access' );

jimport( 'joomla.application.component.view' );

/**
 * AuroraSim View
 *
 * @package    Joomla.Tutorials
 * @subpackage Components
 */
class AuroraSimsViewAuroraSim_User extends JView
{
	/**
	 * display method of AuroraSim view
	 * @return void
	 **/
	function display($tpl = null)
	{
		//get the aurorasim_user data
		$aurorasim_user		=& $this->get('Data');
		// check to see if its less than 0
		$isNew	= ($aurorasim_user->id < 1);
		
		// add close or cancel button
		$text = $isNew ? JText::_( 'New' ) : JText::_( 'Edit' );
		JToolBarHelper::title(   JText::_( 'AuroraSim_User' ).': <small><small>[ ' . $text.' ]</small></small>' );
		JToolBarHelper::save();
		if ($isNew)  {
			JToolBarHelper::cancel();
		} else {
			// for existing items the button is renamed `close`
			JToolBarHelper::cancel( 'cancel', 'Close' );
		}
		
		// adding the refence so for the template
		$this->assignRef('aurorasim_user',		$aurorasim_user);
		parent::display($tpl);
	}
}
