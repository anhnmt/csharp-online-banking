(function($) {
	
	var direction =  getUrlParams('dir');
	if(direction != 'rtl')
	{direction = 'ltr'; }

	var dlabSettingsOptions = {
		typography: "poppins",
		version: "light",
		layout: "horizontal",
		headerBg: "color_1",
		navheaderBg: "color_1",
		sidebarBg: "color_1",
		sidebarStyle: "full",
		sidebarPosition: "static",
		headerPosition: "static",
		containerLayout: "mini",
		direction: direction
	};
		
	new dlabSettings(dlabSettingsOptions); 

	jQuery(window).on('resize',function(){
		new dlabSettings(dlabSettingsOptions); 
	});

})(jQuery);