<?php
/**
 * AuroraSim World table class
 * 
 * @package    Joomla.Tutorials
 * @subpackage Components
 * @link http://docs.joomla.org/Developing_a_Model-View-Controller_Component_-_Part_4
 * @license		GNU/GPL
 */

// No direct access
defined( '_JEXEC' ) or die( 'Restricted access' );

/**
 * AuroraSim Table class
 *
 * @package    Joomla.Tutorials
 * @subpackage Components
 */
class TableAuroraSim_user extends JTable
{
	/**
	 * Primary Key
	 *
	 * @var int
	 */
	var $id = null;

	/**
	 * @var string
	 */
	var $uuid = null;
	
	/**
	 *
	 * @var int
	 */
	var $joomla_userid = null;

	/**
	 * Constructor
	 *
	 * @param object Database connector object
	 */
	function TableAuroraSim_user(& $db) {
		parent::__construct('#__aurorasim_user', 'id', $db);
	}
}
