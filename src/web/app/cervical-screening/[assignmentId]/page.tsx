import ScreeningPage, {
  generateMetadata,
} from "@/app/components/screeningPage";

export const metadata = generateMetadata("Cervical");

export default function CervicalScreening(props: {
  params: Promise<{ assignmentId: string }>;
}) {
  return (
    <ScreeningPage
      metadataTitle={"Cervical"}
      screeningType="Cervical"
      {...props}
    />
  );
}
