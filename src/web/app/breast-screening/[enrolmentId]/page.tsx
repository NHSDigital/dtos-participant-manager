import ScreeningPage, {
  generateMetadata,
} from "@/app/components/screeningPage";

export const metadata = generateMetadata("Breast");

interface PageProps {
  readonly params: Promise<{ readonly enrolmentId: string }>;
}

export default function BreastScreening(props: PageProps) {
  return <ScreeningPage screeningType="Breast" {...props} />;
}
