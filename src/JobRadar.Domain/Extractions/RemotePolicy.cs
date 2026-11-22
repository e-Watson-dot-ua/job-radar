namespace JobRadar.Domain.Extractions;

/// <summary>
/// Where the work is done, as stated in the posting; <c>null</c> when not stated, never a guess:
/// Onsite - office only; Hybrid - office and remote mix; Remote - fully remote.
/// </summary>
public enum RemotePolicy { Onsite, Hybrid, Remote }
