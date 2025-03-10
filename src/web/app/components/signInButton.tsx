import { getAuthConfig } from "@/app/lib/auth";
import styles from "@/app/styles/components/signIn.module.scss";

export default function SignInButton() {
  return (
    <form
      action={async () => {
        "use server";
        const { signIn } = await getAuthConfig();
        await signIn("nhs-login", { callbackUrl: '/screening' });
      }}
    >
      <button
        className={`nhsuk-button app-button--login ${styles["app-button--login"]} nhsuk-u-margin-bottom-4`}
        data-module="nhsuk-button"
        type="submit"
      >
        <svg
          className="nhsuk-logo"
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 80 32"
          height="27"
          aria-hidden="true"
          focusable="false"
        >
          <path
            fill="currentColor"
            d="M80 0v32H0V0h80ZM69 2.2c-5.8 0-11.6 2-11.6 8.8 0 7.4 10.2 5.8 10.2 10 0 2.6-3.4 3-5.6 3-2.2 0-5-.6-6.4-1.4L54 28c2.2.8 5.4 1.4 8 1.4 6.2 0 12.8-1.8 12.8-9 0-7.8-10.2-6.6-10.2-10.2 0-2.2 2.2-2.6 5-2.6 2.6 0 4.4.6 5.8 1.2L77 3.4c-1.8-.8-4.8-1.2-8-1.2ZM16.6 3H7.8L2.2 29h6.6l3.6-18h.2L18 29h8.6l5.6-26h-6.6L22 21h-.2L16.6 3Zm25.2 0h-7.2l-5.2 26h6.8l2.4-11.2h8.2L44.6 29h7L57 3h-7l-2.2 9.8h-8l2-9.8Z"
          ></path>
        </svg>

        <div>
          <span>Continue with</span> NHS login
        </div>
      </button>
    </form>
  );
}
