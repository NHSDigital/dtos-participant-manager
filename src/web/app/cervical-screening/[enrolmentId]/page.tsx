import ScreeningPage, {
  generateMetadata,
} from "@/app/components/screeningPage";

export const metadata = generateMetadata("Cervical");

interface PageProps {
  readonly params: Promise<{ readonly assignmentId: string }>;
}

export default function CervicalScreening(props: PageProps) {
  return <ScreeningPage screeningType="Cervical" {...props} />;
}
