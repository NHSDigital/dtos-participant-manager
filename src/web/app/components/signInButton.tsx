import { getAuthConfig } from "@/app/lib/auth";
import { cookies } from "next/headers";
import styles from "@/app/styles/components/signIn.module.scss";
import { redirect } from "next/navigation";

async function handleSignIn() {
  "use server";

  if (process.env.APP_ENV === "test") {
    // 🔁 Simulate successful NHS login redirect in test
    const mockVerifier = "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2Q0JDLUhTNTEyIiwia2lkIjoiX0o2LWk1NC10d2ZLNEtZdmxIUkpxWEFnWHNjb1l2bVh5Z2xEbGFRMFF0cjQxdnZtNGhrMWU0UEFPTHN0dUZsak5GUXk2QnpYQUY0SDNqQU1VaXZQNkEifQ..P-PxAzfxtmNJdebFEujRyg.EP3rjItORQc-7kMihrPEUeC9D0WUaJLEknTorXZBrhL1Imik-HXKqJOHT4vDAjifu32tbuGIa5jS5hS6bbVZ1dvdX1DvRmRgdt_vHjHQKFJ8NYGDQ8CXphOewz0CAVKB06YtA_kBN9o1TIjlEVMw82vL6xadf8ztgVMiMnNkGL2pPUBIxMSHLnaenGSL3ssK.1Csy2Rpqevpav4JpsfPH0bfx1vpoCJkEFeAOSkkWRms";

    const cookieStore = await cookies();
    cookieStore.set("__Secure-authjs.pkce.code_verifier", mockVerifier, {
      path: "/",
      httpOnly: true,
      secure: true,
      sameSite: "lax",
    });

    return redirect(
      "https://localhost:3000/api/auth/callback/nhs-login?code=fb66c0ac-38ec-4edb-b204-90623968a3e8"
    );
  }

  const { signIn } = await getAuthConfig();
  await signIn("nhs-login", { redirectTo: "/screening" });
}

export default function SignInButton() {
  return (
    <form action={handleSignIn}>
      <button
        className={`nhsuk-button app-button--login ${styles["app-button--login"]} nhsuk-u-margin-bottom-4`}
        data-module="nhsuk-button"
        type="submit"
      >
        Continue to NHS login
      </button>
    </form>
  );
}
