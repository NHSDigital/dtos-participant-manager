import { getAuthConfig } from "@/app/lib/auth";

export default function SignOutButton() {
  return (
    <form
      action={async () => {
        "use server";
        const { signOut } = await getAuthConfig();
        await signOut({ redirectTo: "/" });
      }}
    >
      <button
        className="nhsuk-button nhsuk-button--secondary"
        data-module="nhsuk-button"
        type="submit"
      >
        Log out
      </button>
    </form>
  );
}
