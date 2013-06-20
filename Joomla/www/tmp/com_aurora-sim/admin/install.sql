DROP TABLE IF EXISTS `#__aurorasim`;

CREATE TABLE `#__aurorasim` (
  `id` int(11) NOT NULL auto_increment,
  `webui_gridname` varchar(36) NOT NULL,
  `webui_url` varchar(256) NOT NULL,
  `webui_texture_url` varchar(256) NOT NULL,
  `webui_password` varchar(36) NOT NULL,
  `isdefault` int(11) NOT NULL,
  `default_home` varchar(36) NOT NULL,
  `aurora_database_type` varchar(256) NOT NULL,
  `aurora_database_host` varchar(256) NOT NULL,
  `aurora_database_name` varchar(256) NOT NULL,
  `aurora_database_user` varchar(256) NOT NULL,
  `aurora_database_pass` varchar(256) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;

INSERT INTO `#__aurorasim` (`webui_gridname`,`webui_url`,`webui_texture_url`,`webui_password`, `isdefault`, `default_home`, `aurora_database_type`, `aurora_database_host`, `aurora_database_name`, `aurora_database_user`, `aurora_database_pass` ) VALUES ('Grid Name', 'http://your_webui_server_ip_or_dns:8007/WIREDUX', 'http://your_webui_server_ip_or_dns:8002', '***', 1, '00000000-0000-0000-0000-000000000000', 'mysql', '127.0.0.1', 'aurora-sim', 'aurora', '***');


DROP TABLE IF EXISTS `#__aurorasim_user`;

CREATE TABLE `#__aurorasim_user` (
  `id` int(11) NOT NULL auto_increment,
  `uuid` varchar(36) NOT NULL,
  `joomla_userid` int(11) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;

