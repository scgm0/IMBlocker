using Vintagestory.API.Client;

namespace IMBlocker;

public interface IIMEHandler {
	bool ImeEnabled { get; }
	void Initialize(ICoreClientAPI api);
	void SyncState();
	void EnableIME();
	void DisableIME();
}