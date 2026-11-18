// SWARMSHIFT: stops and restarts the browser frame loop, so a paused game draws no frames.
mergeInto(LibraryManager.library, {
	PauseMainLoop: function () {
		Module["pauseMainLoop"]();
	},
	ResumeMainLoop: function () {
		Module["resumeMainLoop"]();
	},
});
