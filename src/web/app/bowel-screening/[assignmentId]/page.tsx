import ScreeningPage, {
  generateMetadata,
} from "@/app/components/screeningPage";

export const metadata = generateMetadata("Bowel");

export default function BowelScreening(props: {
  params: Promise<{ assignmentId: string }>;
}) {
  return (
    <ScreeningPage metadataTitle={"Bowel"} screeningType="Bowel" {...props} />
  );
}
