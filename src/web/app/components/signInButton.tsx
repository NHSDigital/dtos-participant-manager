import { getAuthConfig } from "@/app/lib/auth";
import styles from "@/app/styles/components/signIn.module.scss";

export default function SignInButton() {
  return (
    <form
      action={async () => {
        "use server";
        const { signIn } = await getAuthConfig();
        await signIn("nhs-login", { redirectTo: "/screening" });
      }}
    >
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
