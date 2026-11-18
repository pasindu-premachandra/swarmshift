/**
 * Copyright 2023 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// Changed for SWARMSHIFT: save, load and interstitial results go back to C#, and callbacks use Module.SendMessage.

mergeInto(LibraryManager.library, {
    GetYTGameSDKVersion: function () {
		var sdkVersion = ytgame.SDK_VERSION;
		var bufferSize = lengthBytesUTF8(sdkVersion) + 1;
		var strBuffer = _malloc(bufferSize);
		stringToUTF8(sdkVersion, strBuffer, bufferSize);
		return strBuffer;
    },
	SendYTGameScore: function (score) {
		ytgame.engagement.sendScore({value: score});
	},
	SendFirstFrameReady: function () {
		ytgame.game.firstFrameReady();
	},
	SendGameReady: function () {
		ytgame.game.gameReady();
	},
	SaveGameData: function (playableSaveData) {
		ytgame.game.saveData(UTF8ToString(playableSaveData)).then(() => {
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnSaveDataEvent", "true");
		}, (e) => {
			// Send an error to YouTube
			ytgame.health.logError();
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnSaveDataEvent", "false");
		});
	},
	LoadGameData: function () {
		ytgame.game.loadData().then((data) => {
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnLoadSaveEvent", data == null ? "" : String(data));
		}, (e) => {
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnLoadSaveFailedEvent");
		});
	},
	SendGameError: function (errorMessage) {
		console.error(errorMessage);
		// Send an error to YouTube
		ytgame.health.logError();
	},
	SendGameWarning: function (warningMessage) {
		console.debug(warningMessage);
		// Send an warning to YouTube
		ytgame.health.logWarning();
	},
	CheckIsAudioEnabled: function () {
		var isEnabled = Boolean(ytgame.system.isAudioEnabled());
		return isEnabled;
	},
	SetCallbackOnAudioEnabledChange: function () {
		const unsetCallback = ytgame.system.onAudioEnabledChange((isAudioEnabled) => {
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnAudioEnabledChange", isAudioEnabled.toString());
		 });
	},
	SetCallbackOnGamePause: function () {
		const unsetCallback = ytgame.system.onPause(() => {
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnPauseEvent");
		 });
	},
	SetCallbackOnGameResume: function () {
		const unsetCallback = ytgame.system.onResume(() => {
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnResumeEvent");
		 });
	},
	CheckInPlayablesEnv: function () {
		var isPlayables = Boolean(typeof ytgame !== 'undefined' && ytgame.IN_PLAYABLES_ENV);
		return isPlayables;
	},
	RequestAnInterstitialAd: function () {
		ytgame.ads.requestInterstitialAd().then(() => {
			// Ad request successful, pause and mute calls should also be sent to your game.
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnInterstitialAdEvent", "true");
		}, (e) => {
			// Ad not shown as there is a delay or none are available. Other failures will be picked up by YT.
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnInterstitialAdEvent", "false");
		});
	},
	RequestAnRewardedAd: async function (rewardId) {
		var id = UTF8ToString(rewardId);
		try {
			const isRewardEarned = await ytgame.ads.requestRewardedAd(id);
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnRewardedAdEvent", Boolean(isRewardEarned).toString());
		} catch (error) {
			Module["SendMessage"]("YTGameWrapper", "ReceiveOnRewardedAdEvent", "false");
		}
	},
});
