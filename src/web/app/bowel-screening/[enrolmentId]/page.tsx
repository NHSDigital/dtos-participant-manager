import ScreeningPage, {
  generateMetadata,
} from "@/app/components/screeningPage";

export const metadata = generateMetadata("Bowel");

interface PageProps {
  readonly params: Promise<{ readonly enrolmentId: string }>;
}

export default function BowelScreening(props: PageProps) {
  return <ScreeningPage screeningType="Bowel" {...props} />;
}
