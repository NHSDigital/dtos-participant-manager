import type { EligibilityItem } from "@/app/types";
import { createUrlSlug } from "@/app/lib/utils";
import Card from "@/app/components/card";
import InsetText from "@/app/components/insetText";

interface ScreeningListProps {
  readonly eligibility: readonly EligibilityItem[] | null;
}

export default function ScreeningList({ eligibility }: ScreeningListProps) {
  if (!eligibility?.length) {
    return <InsetText text="You have no screening assignments." />;
  }

  return (
    <>
      {eligibility.map((item) => {
        const url = `${createUrlSlug(item.screeningName)}/${item.enrolmentId}`;
        return (
          <Card key={item.enrolmentId} title={item.screeningName} url={url} />
        );
      })}
    </>
  );
}
