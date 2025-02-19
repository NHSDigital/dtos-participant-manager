export interface EligibilityItem {
  assignmentId: string;
  screeningName: string;
}

export interface PathwayItem {
  assignmentId: string;
  assignmentDate: string;
  status: "Active" | "Inactive";
  nextActionDate: string;
  screeningName: string;
  pathwayName: string;
  infoUrl: string;
}

export type EligibilityResponse = EligibilityItem[];
export type PathwayResponse = PathwayItem;
