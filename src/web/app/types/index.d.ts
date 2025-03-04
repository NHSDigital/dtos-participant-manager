export interface EligibilityItem {
  enrolmentId: string;
  screeningName: string;
}

export interface EnroledPathwayItem {
  enrolmentId: string;
  enrolmentDate: string;
  status: "Active" | "Inactive";
  nextActionDate: string;
  screeningName: string;
  pathwayTypeName: string;
  infoUrl: string;
}

export type EligibilityResponse = EligibilityItem[];
export type PathwayResponse = EnroledPathwayItem;
