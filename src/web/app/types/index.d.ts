export interface EligibilityItem {
  assignmentId: string;
  participantId: string;
  pathwayId: string;
  assignmentDate: string;
  lapsedDate: string;
  status: "Active" | "Inactive";
  nextActionDate: string;
  participant: string;
  pathwayTypeId: string;
  episodes: string;
  screeningName: string;
  pathwayName: string;
}

export type EligibilityResponse = EligibilityItem[];
