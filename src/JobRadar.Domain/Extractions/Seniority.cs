namespace JobRadar.Domain.Extractions;

/// <summary>
/// Seniority level as stated in the posting; <c>null</c> when not stated, never a guess:
/// Intern - internship or working student; Junior - entry level; Middle - mid level ("regular");
/// Senior - senior individual contributor; Lead - team or tech lead; Principal - principal, staff, or architect.
/// </summary>
public enum Seniority { Intern, Junior, Middle, Senior, Lead, Principal }
