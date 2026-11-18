// SWARMSHIFT: the Facebook Instant Games calls the Meta build makes. Results go back to the YTGameWrapper object, where MetaBridge listens.
mergeInto(LibraryManager.library, {
	MetaLoadData: function () {
		FBInstant.player.getDataAsync(["save"]).then((data) => {
			Module["SendMessage"]("YTGameWrapper", "ReceiveMetaLoaded", data && data.save != null ? String(data.save) : "");
		}, (e) => {
			console.warn(e);
			Module["SendMessage"]("YTGameWrapper", "ReceiveMetaLoadFailed");
		});
	},
	MetaSaveData: function (json) {
		FBInstant.player.setDataAsync({ save: UTF8ToString(json) }).then(() => {
			Module["SendMessage"]("YTGameWrapper", "ReceiveMetaSaved", "true");
		}, (e) => {
			console.warn(e);
			Module["SendMessage"]("YTGameWrapper", "ReceiveMetaSaved", "false");
		});
	},
	MetaListenForPause: function () {
		FBInstant.onPause(() => {
			Module["SendMessage"]("YTGameWrapper", "ReceiveMetaPause");
		});
		document.addEventListener("visibilitychange", () => {
			if (document.visibilityState === "visible") {
				Module["SendMessage"]("YTGameWrapper", "ReceiveMetaResume");
			}
		});
	},
});
