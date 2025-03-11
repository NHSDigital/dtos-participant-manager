import ScreeningPage, {
  generateMetadata,
} from "@/app/components/screeningPage";

export const metadata = generateMetadata("Breast");

export default function BreastScreening(props: {
  params: Promise<{ assignmentId: string }>;
}) {
  return (
    <ScreeningPage metadataTitle={"Breast"} screeningType="Breast" {...props} />
  );
}
