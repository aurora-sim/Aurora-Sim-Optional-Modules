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
class TableAuroraSim extends JTable
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
	var $webui_gridname = null;

	/**
	 * @var string
	 */
	var $webui_url = null;
	
	/**
	 * @var string
	 */
	var $webui_texture_url = null;
	
	/**
	 * @var string
	 */
	var $webui_password = null;

	/**
	 * @var int
	 */
	var $isdefault = null;
	
	/**
	 * @var string
	 */
	var $default_home = null;
	
		/**
	 * @var string
	 */
	var $aurora_database_type = null;
	
		/**
	 * @var string
	 */
	var $aurora_database_host = null;
	
		/**
	 * @var string
	 */
	var $aurora_database_name = null;
	
		/**
	 * @var string
	 */
	var $aurora_database_user = null;
	
		/**
	 * @var string
	 */
	var $aurora_database_pass = null;

	/**
	 * Constructor
	 *
	 * @param object Database connector object
	 */
	function TableAuroraSim(& $db) {
		parent::__construct('#__aurorasim', 'id', $db);
	}
}
